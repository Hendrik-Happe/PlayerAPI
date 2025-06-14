namespace PlayerAPI.Services
{
    public interface IVolumeHandler
    {
        public void IncreaseVolume();

        public void DecreaseVolume();

        public byte GetVolume();
    }
}
