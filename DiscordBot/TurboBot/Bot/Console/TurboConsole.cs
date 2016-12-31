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
        DiscordClient BotClient { get; set; }
        bool Connected { get; set; }

        string LogFile { get; set; }
        public TurboConsole(IBot bot) {
            this.LogFile = "TurboConsole.Log.txt";
            this.BotClient = bot.CurrentBot;
            this.Bot = bot;
            Log($"Console Initialized at: {DateTime.Now.ToLongTimeString()}");
        }

        public void Run() {
            new Task(() => CheckConnection((Bot) Bot)).Start();
            while (true) {

                string input = System.Console.ReadLine();
                if (input != "" && input[0] == '#') {
                    ProcessCommand(input.Remove(0, 1).Split(' '));
                }

                if (input == "quit") {
                    break;
                }
            }
        }

        private void ProcessCommand(string[] input) {
            if (input[0].ToLower() == "join") {
                if (input.Length < 2) {
                    Log("Parameter Error: Given empty parameter");
                    return;
                }
                try {
                    Bot.JoinChannel(input[1]);
                } catch (NullReferenceException) {
                    Log("Join Error: The chosen channel does not exist");
                }
            }
        }

        public void Log(string text) {
            File.AppendAllText(this.LogFile, $"{DateTime.Now.ToLongTimeString()}\t-\t{text}");
            System.Console.WriteLine(text);
        }

        private void CheckConnection(Bot bot) {
            while (true) {
                Thread.Sleep(15);
                
                if (bot.Client != null) {
                    bot.Client.CancelToken.ThrowIfCancellationRequested();
                }
            }
        }
    }
}
