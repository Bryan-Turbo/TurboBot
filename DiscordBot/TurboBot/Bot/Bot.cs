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
            TurboConsole.Log("Bot Connected!");
        }

        public async void JoinChannel(string serverName) {
            if (this.IsInChannel) {
                await this.CurrentBot.GetService<AudioService>().Leave(this.CurrentVoiceChannel);
            }
            var servers = this.CurrentBot.FindServers(serverName);
            this.CurrentVoiceChannel = servers.FirstOrDefault().VoiceChannels.FirstOrDefault();
            this.Client = await this.CurrentBot.GetService<AudioService>().Join(this.CurrentVoiceChannel);
            this.Player = new YoutubePlayer(this.CurrentChannel, this.CurrentVoiceChannel);
        }

        public void Run() {
            this.CurrentBot.MessageReceived += MessageEvent;
        }

        private async void MessageEvent(object sender, MessageEventArgs e) {
            this.CurrentChannel = e.Channel;
            if (e.Message.RawText[0] == '#') {
                this.ProcessCommand(e.Message.RawText.Remove(0, 1));
            }
        }

        private void ProcessCommand(string commandText) {
            string command = commandText.Split(' ')[0];
            if (command == "play") {
                if (this.MusicPlayerTask != null && this.MusicPlayerTask.Status == TaskStatus.Running) {
                    this.CurrentChannel.SendMessage("As you can **CLEARLY HEAR**, some music i still playing\nconsider **USING THIS COMMAND** to stop the current song: \"#stop\"");
                    return;
                }
                this.MusicPlayerTask = new Task(() => this.Player.PlayMusic(commandText.Split(' ')[1], 2, this.Client));
                this.MusicPlayerTask.Start();
            }
            if (command == "stop") {
                YoutubePlayer.StopPlaying(this.Player);
            }
        }
    }
}
