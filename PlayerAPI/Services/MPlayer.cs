using Mono.TextTemplating;
using PlayerAPI.Models;
using PlayerAPI.Models.Config;
using System.Diagnostics;

namespace PlayerAPI.Services
{
    public class MPlayer : IMyPlayer, IDisposable
    {
        private readonly string fifoFile;

        private enum PlayerStatus
        {
            Playing,
            Paused,
            Stopped
        }

        private PlayerConfig playerConfig;

        private PlayList? currentPlaylist = null;

        private int currentSong = 0;

        private IGPIOController gpioController;

        private ILogger<MPlayer> logger;

        private Process? Player;

        private PlayerStatus status = PlayerStatus.Stopped;

        public MPlayer(PlayerConfig playerConfig, IGPIOController gpioController, ILogger<MPlayer> logger)
        {
            this.playerConfig = playerConfig;
            this.logger = logger;
            this.gpioController = gpioController;

            fifoFile = Path.GetFullPath(playerConfig.FifoFile);

            MakeFifoFile();

            logger.LogInformation($"Using FIFO file: {fifoFile}");
        }

        private void MakeFifoFile()
        {
            logger.LogInformation("Creating FIFO file...");
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "mkfifo",
                Arguments = fifoFile,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (process != null)
            {
                process.WaitForExit();
            }
            else
            {
                logger.LogError("Failed to start mkfifo process.");
                throw new InvalidOperationException("Failed to start the process for creating FIFO file.");
            }

            if (!System.IO.File.Exists(fifoFile))
            {
                logger.LogError($"FIFO file {fifoFile} was not created successfully. Make sure the have the permission to create this file");
                throw new InvalidOperationException($"FIFO file {fifoFile} was not created successfully.");
            }

            logger.LogInformation("FIFO file created successfully.");
        }

        private void HandleEndOfSong(object? sender, EventArgs eventArgs)
        {
            if (Player != null && Player.HasExited)
            {
                Player = null;
            }
                
            Next();
        }

        public void Next()
        {
            if (currentPlaylist == null)
                return;

            if (currentSong == currentPlaylist.Files.Count)
                return;

            currentSong++;
	    logger.LogInformation($"Next tracknumber: {currentSong} of {currentPlaylist.Files.Count}");
            if (currentSong == currentPlaylist.Files.Count)
            {
                if (status != PlayerStatus.Stopped)
                {
                    SendStop();
                }
                return;
            }

            PlayCurrentSong();
        }

        public void Pause()
        {
            if (currentPlaylist == null || Player == null || status != PlayerStatus.Playing)
                return;

            SendPause();
        }

        private void SendPause()
        {
            status = PlayerStatus.Paused;
            logger.LogInformation("Pausing playback.");
            System.IO.File.WriteAllText(fifoFile, "pause\n");
            gpioController.HidePlaying();
        }

        public void Play(PlayList playList)
        {
            if (playList == null)
                return;

            if (playList == currentPlaylist)
            {
                if (status == PlayerStatus.Paused)
                {
                    Resume();
                }
                else if (status == PlayerStatus.Stopped)
                {
		    if (currentSong == playList.Files.Count)
		    {
			currentSong = 0;
		    }
                    PlayCurrentSong();
                }
                return;
            }

            logger.LogInformation($"Playing playlist: {playList.Name} with {playList.Files.Count} files.");

            currentPlaylist = playList;
            currentSong = 0;

            PlayCurrentSong();
        }

        public void Previous()
        {
            if (currentPlaylist == null)
                return;

            if (currentSong == -1)
                return;

            currentSong--;

            if (currentSong == -1)
            {
                if (status != PlayerStatus.Stopped)
                {
                    SendStop();
                }
                return;
            }

            PlayCurrentSong();
        }

        private void SendStop(bool hidePlaying = true)
        {
            if (Player != null)
            {
                Player.EnableRaisingEvents = false;
                Player.Exited -= HandleEndOfSong;
                System.IO.File.WriteAllText(fifoFile, "quit\n");
                Player.Kill();
                Player.Dispose();
                Player = null;
            }

            status = PlayerStatus.Stopped;

            if (hidePlaying)
                gpioController.HidePlaying();
        }

        public void Resume()
        {
            if (currentPlaylist == null || status != PlayerStatus.Paused)
                return;

            gpioController.ShowPlaying();
            SendResume();
        }

        private void SendResume()
        {
            status = PlayerStatus.Playing;
            System.IO.File.WriteAllText(fifoFile, "pause\n");
        }

        private void PlayCurrentSong()
        {
            var filepath = GetFilePath();

            logger.LogInformation($"Play song: {filepath}");
            logger.LogInformation($"File exists: {Path.Exists(filepath)}");

            gpioController.ShowPlaying();

            if (Player != null)
            {
                SendStop(false);
            }

            logger.LogInformation($"Start playing {filepath}");

            Player = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "mplayer",
                    Arguments = $" -slave -input file=\"{fifoFile}\" -quiet \"{filepath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            Player.Exited += HandleEndOfSong;
            Player.Start();
            status = PlayerStatus.Playing;
        }

        private string GetFilePath()
        {
            if (currentPlaylist == null)
            {
                throw new InvalidOperationException("No current playlist.");
            }

            if (currentSong < 0 || currentSong >= currentPlaylist.Files.Count)
            {
                throw new InvalidOperationException($"{nameof(currentSong)} out of bounds: {currentSong}");
            }

            return Path.Combine(playerConfig.BasePath, currentPlaylist.Files[currentSong].Filename);
        }

        public void Dispose()
        {
            SendStop();
            System.IO.File.Delete(fifoFile);
        }
    }
}
