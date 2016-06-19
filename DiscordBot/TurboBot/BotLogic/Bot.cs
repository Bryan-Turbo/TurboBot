using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace TurboBot.BotLogic {
    class Bot : IBot{
        DiscordClient TurboBot;

        public Bot(InitialBot bot) {
            TurboBot = bot.GetBot();
        }

        public void Run() {
            TurboBot.MessageReceived += TurboBot_MessageReceived;
            TurboBot.Wait();
        }

        private void TurboBot_MessageReceived(object sender, MessageEventArgs e) {
            if (e.Message.RawText.ToLower().Contains("what")) {
                e.Channel.SendMessage("YOU MAY NOT SAY ANYTHING");
            }
        }
    }
}
