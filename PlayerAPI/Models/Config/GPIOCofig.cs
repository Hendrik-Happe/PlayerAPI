namespace PlayerAPI.Models.Config
{
    public class GPIOCofig
    {
        public int StatusLED { get; set; } = -1;

        public int PowerLED { get; set; } = -1;

        public int NextButton { get; set; } = -1;

        public int PreviousButton { get; set; } = -1;

        public int VolumeUpButton { get; set; } = -1;

        public int VolumeDownButton { get; set; } = -1;

        public int PauseButton { get; set; } = -1;

        public int PlayButton { get; set; } = -1;
    }
}
