namespace PlayerAPI.Models.Config
{
    public sealed class PlayerConfig
    {
        public string BasePath { get; set; } = ".";
        public string FifoFile { get; set; } = ".player-fifo";
    }
}
