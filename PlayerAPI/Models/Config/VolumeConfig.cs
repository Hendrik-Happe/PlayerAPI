namespace PlayerAPI.Models.Config
{
    public class VolumeConfig
    {
        public byte MaxVolume { get; set; } = 100;
        public byte MinVolume { get; set; } = 0;
        public byte DefaultVolume { get; set; } = 30;
        public byte VolumeStep { get; set; } = 5;
    }
}
