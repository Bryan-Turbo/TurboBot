using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;

namespace TurboBot.Bot.Console {
    class TurboConsole {
        public IBot Bot { get; set; }
        public DiscordClient BotClient { get; set; }
        public bool Connected { get; set; }

        string LogFile { get; set; }
        public TurboConsole(IBot bot) {
            this.LogFile = "TurboConsole.Log.txt";
            this.BotClient = bot.CurrentBot;
            this.Bot = bot;
            Log($"Console Initialized at: {DateTime.Now.ToLongTimeString()}");
            new Task(() => JoinVoiceChannel(194151550300454914)).Start();
            //new Task(() => JoinVoiceChannel(234804942706049025)).Start();
        }

        public void Run() {
            while (true) {

                string input = System.Console.ReadLine();
                if (input != "" && input[0] == '#') {
                    ProcessCommand(input.Remove(0, 1).Split(' '));
                }



                if (input == "quit") {
                    this.BotClient.Disconnect();
                    break;
                }
            }
        }

        private async void JoinVoiceChannel(ulong channel) {
            while (true) {
                if (this.Connected) {
                    bool joinedChannel = await this.Bot.JoinChannel(channel);
                    if (joinedChannel) {
                        this.Log("Bot connected to VoiceChannel!");
                        return;
                    }
                }
            }
        }

        private void ProcessCommand(string[] input) {
            
        }

        public void Log(string text) {
            File.AppendAllText(this.LogFile, $"{DateTime.Now.ToLongTimeString()}\t-\t{text}\n");
            System.Console.WriteLine(text);
        }
    }
}
