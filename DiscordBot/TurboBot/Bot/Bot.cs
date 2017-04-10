using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Discord.Net;
using Discord.Audio;
using Discord;
using TurboBot.Bot.Console;
using TurboBot.Bot.Music;

namespace TurboBot.Bot {
    class Bot : IBot{
        public DiscordClient CurrentBot { get; set; }
        public Channel CurrentChannel { get; set; }
        public Channel CurrentVoiceChannel { get; set; }
        public bool IsInChannel { get; set; }

        bool IsLocked { get; set; }
        string Locker { get; set; }

        Thread TurboConsoleThread { get; set; }
        Task MusicPlayerTask { get; set; }

        TurboConsole TurboConsole { get; set; }
        IPlayer Player { get; set; }
        public IAudioClient Client { get; set; }

        public Bot() {
            this.TurboConsole = new TurboConsole(this);
            this.TurboConsoleThread = new Thread(() => this.TurboConsole.Run());
            this.TurboConsoleThread.Start();

            if (!File.Exists("config.token")) {
                TurboConsole.Log("\"config.token\" does not exist...creating");
                File.Create("config.token");
                MessageBox.Show("Please enter your token in \"config.token\" then start this application again", "Enter Bot token", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Environment.Exit(0);
            }
            string token = File.ReadAllText("config.token");
            if (token.Length != 59)
                throw new Exception("Invalid Token");

            this.CurrentBot = new DiscordClient();

            this.ConnectBot(token);
        }

        private async void ConnectBot(string token) {
            await this.CurrentBot.Connect(token, TokenType.Bot);
            this.CurrentBot.UsingAudio(x => x.Mode = AudioMode.Outgoing);
            TurboConsole.Connected = true;
            TurboConsole.BotClient = this.CurrentBot;
            TurboConsole.Log("Bot Connected!");
        }

        public async Task<bool> JoinChannel(ulong serverId) {
            if (this.IsInChannel) {
                await this.CurrentBot.GetService<AudioService>().Leave(this.CurrentVoiceChannel);
            }
            var server = this.CurrentBot.GetServer(serverId);
            //var servers = this.CurrentBot.FindServers(serverName).ToList();
            if (server == null) {
                return false;
            }
            try {
                this.CurrentVoiceChannel = server.VoiceChannels.FirstOrDefault(e => e.Name == "General");
                this.Client = await this.CurrentBot.GetService<AudioService>().Join(this.CurrentVoiceChannel);
                this.Player = new YoutubePlayer(this.CurrentChannel, this.CurrentVoiceChannel, this.CurrentBot, this.TurboConsole);
                return true;
            } catch (ArgumentNullException) {
                return false;
            }
        }

        public void Run() {
            this.CurrentBot.MessageReceived += MessageEvent;
        }

        private async void MessageEvent(object sender, MessageEventArgs e) {
            this.CurrentChannel = e.Channel;
            this.Player.Channel = e.Channel;
            if (e.Message.RawText == "") {
                return;
            }
            if (e.Message.RawText[0] == '#') {
                this.ProcessCommand(e.Message.RawText.Remove(0, 1), e);
            } else {
                if (e.Message.RawText.Contains("(╯°□°）╯︵ ┻━┻") || e.Message.RawText.Contains("(╯°□°）╯︵ ┻┻")) {
                    await this.CurrentChannel.SendMessage("HEY, let's keep it clean here...  ┬─┬﻿ ノ( ゜-゜ノ)");
                }
            }
        }

        private void ProcessCommand(string commandText, MessageEventArgs e) {
            string command = commandText.Split(' ')[0];
            if (command == "play") {
                if (IsLocked) {
                    if (this.Locker != e.User.Name && e.User.Name != "Bryan-Turbo") {
                        this.CurrentChannel.SendMessage("LEL YOU SCRUB, YOU ARE NOT AUTHORIZED TO DO DIS");
                        return;
                    }
                }
                if (this.MusicPlayerTask != null && this.MusicPlayerTask.Status == TaskStatus.Running) {
                    this.CurrentChannel.SendMessage("As you can **CLEARLY HEAR**, some music i still playing\nconsider **USING THIS COMMAND** to stop the current song: \"#stop\"");
                    return;
                }
                if (commandText.Split(' ').Length != 2) {
                    this.CurrentChannel.SendMessage("I CANNOT PLAY MUSIC (or something else...( ͡° ͜ʖ ͡°)) IF YOU DON'T **GIVE ME A PROPER FUCKING LINK**...jesus christ man");
                    return;
                }
                if (!commandText.Split(' ')[1].Contains("www.youtube.com/watch?v=")) {
                    this.CurrentChannel.SendMessage("I CANNOT PLAY MUSIC (or something else...( ͡° ͜ʖ ͡°)) IF YOU DON'T **GIVE ME A PROPER FUCKING LINK**...jesus christ man");
                    return;
                }
                this.MusicPlayerTask = new Task(() => this.Player.PlayMusic(commandText.Split(' ')[1], 2, this.Client));
                this.MusicPlayerTask.Start();
            }
            if (command == "lock") {
                if (this.IsLocked) {
                    if (e.User.Name == Locker || e.User.Name == "Bryan-Turbo") {
                        this.CurrentChannel.SendMessage("Lock released");
                        this.IsLocked = false;
                    }
                } else {
                    this.IsLocked = true;
                    this.Locker = e.User.Name;
                    this.CurrentChannel.SendMessage($"Lock requested by {this.Locker}");
                }
            }
            if (command == "stop") {
                if (IsLocked) {
                    if (this.Locker != e.User.Name && e.User.Name != "Bryan-Turbo") {
                        this.CurrentChannel.SendMessage("LEL YOU SCRUB, YOU ARE NOT AUTHORIZED TO DO DIS");
                        return;
                    }
                }
                YoutubePlayer.StopPlaying(this.Player);
            }
            if (command == "volume") {
                if (IsLocked) {
                    if (this.Locker != e.User.Name && e.User.Name != "Bryan-Turbo") {
                        this.CurrentChannel.SendMessage("LEL YOU SCRUB, YOU ARE NOT AUTHORIZED TO DO DIS");
                        return;
                    }
                }
                string[] okai = commandText.Split(' ');
                int value = 0;
                if (okai.Length == 2) {
                    if (int.TryParse(okai[1], out value)) {
                        this.Player.Volume = value;
                        return;
                    }
                    this.CurrentChannel.SendMessage("ICANNIOTCHANGETHEVOLUMESINCETHISVALUEISINVALID...skrup");
                } else {
                    this.CurrentChannel.SendMessage("I CANNOT CHANGE MY VOLUME IF YOU DON'T **GIVE ME A VALUE**");
                }
            }
            if (command == "insult") {
                string[] parameters = commandText.Split(' ');
                if (parameters.Length != 2) {
                    return;
                }
                string accountName;
                var account = parameters[1];
                if (account.StartsWith("@")) {
                    var users = this.CurrentChannel.FindUsers(account).ToList();
                    accountName = users.Any() ? users[0].Name : account;
                } else {
                    accountName = account;
                }
                this.CurrentChannel.SendMessage($"{accountName} IS A BIG PILE OF SHIT");
            }
        }
    }
}
