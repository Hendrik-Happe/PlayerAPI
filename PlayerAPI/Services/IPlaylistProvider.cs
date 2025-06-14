using PlayerAPI.Models;

namespace PlayerAPI.Services
{
    public interface IPlaylistProvider
    {
        public List<PlayList> GetAllPlaylists();

        public bool TryGetPLaylist(int id, out PlayList? playList);

        void Reload();
    }
}
