using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;

namespace TurboBot.Bot.Music {
    interface IPlayer {
        void PlayMusic(string link, int channelCount, IAudioClient voiceClient);
        
        Channel Channel { get; set; }
        Channel VoiceChannel { get; set; }

        bool CanPlay { get; set; }
        int Volume { get; set; }
    }
}
