using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Audio;

namespace TurboBot.Bot.Music {
    interface IPlayer {
        void PlayMusic(string link, int channelCount, IAudioClient voiceClient);
        //void StopPlaying();
        bool CanPlay { get; set; }
    }
}
