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
using YoutubeExtractor;

namespace TurboBot.Bot.Music {
    class YoutubePlayer : IPlayer{
        string MainDirectory { get; set; }
        Channel Channel { get; set; }
        Channel VoiceChannel { get; set; }
        VideoInfo VideoInfo { get; set; }
        DiscordClient BotClient { get; set; }

        public bool CanPlay { get; set; }
        bool IsPlaying { get; set; }

        public YoutubePlayer(Channel channel, Channel voiceChannel, DiscordClient botClient) {
            this.CanPlay = true;
            this.VoiceChannel = voiceChannel;
            this.BotClient = botClient;
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
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);

            this.VideoInfo = videoInfos.First(info => info.VideoType == VideoType.Mp4 && info.Resolution == 360);
            if (this.VideoInfo.RequiresDecryption) {
                DownloadUrlResolver.DecryptDownloadUrl(VideoInfo);
            }
            var videoDownloader = new VideoDownloader(VideoInfo, Path.Combine(this.MainDirectory, "input" + VideoInfo.VideoExtension));
            //videoDownloader.DownloadProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage);
            videoDownloader.Execute();
        }

        private void ConvertVideoToMp3() {
            Process converter = new Process();
            converter.StartInfo.WorkingDirectory = this.MainDirectory;
            converter.StartInfo.FileName = "ffmpeg.exe";
            converter.StartInfo.Arguments = " -y -i input.mp4 output.mp3";
            converter.Start();
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
                if (voiceClient.State == ConnectionState.Disconnected) {
                    System.Console.WriteLine("SOMETHING WENT FUCKING WRONG, RECONNECTING");
                    voiceClient = await this.BotClient.GetService<AudioService>().Join(this.VoiceChannel);
                }
                this.IsPlaying = true;
                try {
                    voiceClient.Send(buffer, 0, blockSize);
                } catch { /**/}
            }
            voiceClient.Clear();
            this.IsPlaying = false;
            this.CanPlay = true;
        }

        public static void StopPlaying(IPlayer player) {
            player.CanPlay = false;
        }
    }
}
