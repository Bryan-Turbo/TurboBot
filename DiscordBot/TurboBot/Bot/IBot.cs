using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;

namespace TurboBot.Bot {
    public interface IBot {
        DiscordClient CurrentBot { get; set; }
        Channel CurrentChannel { get; set; }
        Channel CurrentVoiceChannel { get; set; }
        bool IsInChannel { get; set; }
        void Run();
        void JoinChannel(string serverName);
    }
}
