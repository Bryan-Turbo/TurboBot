using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using TurboBot.BotLogic;

namespace TurboBot {
    class Program {
        static void Main(string[] args) {
            new Bot(new InitialBot()).Run();
        }
    }
}