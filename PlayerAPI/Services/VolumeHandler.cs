using Microsoft.Extensions.Logging;
using PlayerAPI.Controllers;
using PlayerAPI.Models.Config;
using System.Diagnostics;

namespace PlayerAPI.Services
{
    public class VolumeHandler : IVolumeHandler
    {
        private ILogger<VolumeController> logger;

        private byte currentVolume;

        private readonly VolumeConfig config;

        public VolumeHandler(VolumeConfig config, ILogger<VolumeController> logger)
        {
            this.config = config;

            currentVolume = config.DefaultVolume;

            this.logger = logger;
            ApplyVolume();
        }

        public void IncreaseVolume()
        {
            currentVolume += config.VolumeStep;

            if (currentVolume > config.MaxVolume)
            {
                currentVolume = config.MaxVolume;
            }

            ApplyVolume();
        }

        public byte GetVolume()
        {
            return currentVolume;
        }

        public void DecreaseVolume()
        {
            if (currentVolume < config.VolumeStep + config.MinVolume)
            {
                currentVolume = config.MinVolume;
            }
            else
            {
                currentVolume -= config.VolumeStep;
            }

            ApplyVolume();
        }

        private void ApplyVolume()
        {
            logger.LogInformation($"Update Volume to {currentVolume}");
            StartBashProcess($"amixer -M set 'PCM' {currentVolume}%").WaitForExit();
        }

        public static Process StartBashProcess(string command)
        {
            string text = command.Replace("\"", "\\\"");
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"" + text + "\"",
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            process.Start();
            return process;
        }
    }
}
