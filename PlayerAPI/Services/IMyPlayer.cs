using PlayerAPI.Models;

namespace PlayerAPI.Services
{
    public interface IMyPlayer
    {
        public void Play(PlayList playList);

        public void Pause();

        public void Resume();

        public void Next();

        public void Previous();
    }
}
