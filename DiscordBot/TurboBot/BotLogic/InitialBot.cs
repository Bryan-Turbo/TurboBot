using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace TurboBot.BotLogic {
    class InitialBot{
        DiscordClient Bot;

        public InitialBot() {
            Bot = new DiscordClient();

            Bot.Connect("left empty intentionally", "left empty intentionally");
        }

        public DiscordClient GetBot() {
            return Bot;
        }
    }
}
