using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using NAudio.Wave;
using TurboBot.Bot.Console;
using YoutubeExtractor;

namespace TurboBot.Bot.Music {
    class YoutubePlayer : IPlayer{
        string MainDirectory { get; set; }
        public Channel Channel { get; set; }
        public Channel VoiceChannel { get; set; }
        VideoInfo VideoInfo { get; set; }
        DiscordClient BotClient { get; set; }
        TurboConsole TurboConsole { get; set; }

        public bool CanPlay { get; set; }

        public int Volume {
            get { return this._volume; }
            set {
                if (value <= 100 && value >= 0) {
                    this._volume = value;
                    return;
                }
                TurboConsole.Log("Incorrect volume value given");
                this.Channel.SendMessage("ICANNIOTCHANGETHEVOLUMESINCETHISVALUEISINVALID...skrup");
            }
        }

        private int _volume;
        bool IsPlaying { get; set; }

        public YoutubePlayer(Channel channel, Channel voiceChannel, DiscordClient botClient, TurboConsole console) {
            this.Volume = 10;
            this.CanPlay = true;
            this.Channel = channel;
            this.VoiceChannel = voiceChannel;
            this.BotClient = botClient;
            this.TurboConsole = console;
            string userDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            MainDirectory = Path.Combine(userDirectory, @"Desktop\Test\");
            if (!File.Exists($"{MainDirectory}ffmpeg.exe")) {
                channel.SendMessage("I cannot play music at the moment");
            }
        }

        public void PlayMusic(string link, int channelCount, IAudioClient voiceClient) {
            this.DownloadVideo(link);
            this.ConvertVideoToMp3();
            this.PlayAudio(channelCount, voiceClient);
        }

        private void DownloadVideo(string link) {
            IEnumerable<VideoInfo> videoInfos;
            try {
                videoInfos = DownloadUrlResolver.GetDownloadUrls(link);
            } catch {
                TurboConsole.Log("Incorrect link given");
                this.Channel.SendMessage("I CANNOT PLAY MUSIC (or something else...( ͡° ͜ʖ ͡°)) IF YOU DON'T **GIVE ME A PROPER FUCKING LINK**...jesus christ man");
                return;
            }

            this.VideoInfo = videoInfos.First(info => info.VideoType == VideoType.Mp4 && info.Resolution == 360);
            if (this.VideoInfo.RequiresDecryption) {
                DownloadUrlResolver.DecryptDownloadUrl(VideoInfo);
            }

            var videoDownloader = new VideoDownloader(VideoInfo, Path.Combine(this.MainDirectory, "input" + VideoInfo.VideoExtension));
            //videoDownloader.DownloadProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage);
            videoDownloader.Execute();
        }

        private void ConvertVideoToMp3() {
            Process converter = new Process {
                StartInfo = {
                    WorkingDirectory = this.MainDirectory,
                    FileName = "ffmpeg.exe",
                    Arguments = " -y -i input.mp4 output.mp3",
                    CreateNoWindow = false
                }
            };
            converter.Start();
            TurboConsole.Log("Converting video to audio file");
            converter.WaitForExit();
        }

        private async void PlayAudio(int channelCount, IAudioClient voiceClient) {
            WaveFormat outputFormat = new WaveFormat(48000, 16, channelCount);
            Mp3FileReader reader = new Mp3FileReader($"{this.MainDirectory}output.mp3");
            var resampler = new MediaFoundationResampler(reader, outputFormat);
            resampler.ResamplerQuality = 50;
            int blockSize = outputFormat.AverageBytesPerSecond / 10;
            byte[] buffer = new byte[blockSize];

            while (resampler.Read(buffer, 0, blockSize) > 0 && this.CanPlay) {
                if (voiceClient.State == ConnectionState.Disconnected) 
                    voiceClient = await this.BotClient.GetService<AudioService>().Join(this.VoiceChannel);
                
                this.IsPlaying = true;

                for (int i = 0; i < buffer.Length; i += 2) {
                    short bufferoo = BitConverter.ToInt16(buffer, i);
                    bufferoo = (short) (bufferoo * (Volume / 100f));
                    byte[] tempbuffer = BitConverter.GetBytes(bufferoo);
                    buffer[i] = tempbuffer[0];
                    buffer[i + 1] = tempbuffer[1];
                }

                try {
                    voiceClient.Send(buffer, 0, blockSize);
                } catch {
                    TurboConsole.Log("SOMETHING WENT WRONG");
                }
            }
            voiceClient.Clear();
            reader.Dispose();
            this.IsPlaying = false;
            this.CanPlay = true;
        }

        public static void StopPlaying(IPlayer player) {
            player.CanPlay = false;
        }
    }
}
