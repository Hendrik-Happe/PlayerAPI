using Microsoft.EntityFrameworkCore;
using PlayerAPI.Models;

namespace PlayerAPI.Services
{
    public class PlaylistProvider : IPlaylistProvider
    {
        private FileContext musicContext;

        public PlaylistProvider(FileContext musicContext)
        {
            this.musicContext = musicContext;
        }

        public List<PlayList> GetAllPlaylists()
        {
            var playlists = musicContext.PlayLists.Include(x => x.Files);
            var files = playlists.ToListAsync();
            Task.WaitAll(files);
            var p = files.Result;
            p?.ForEach(x =>
                {
                    if (x != null)
                        x.Files = [.. x.Files.OrderBy(f => f.ID)];
                });

            return p ?? [];
        }

        public bool TryGetPLaylist(int id, out PlayList? playList)
        {
            var playlist = musicContext.PlayLists.Include(x => x.Files)
                .FirstOrDefaultAsync(m => m.ID == id);

            Task.WaitAll(playlist);

            playList = playlist.Result;

            if (playList == null)
                return false;

            playList.Files = [.. playList.Files.OrderBy(x => x.ID)];

            return true;
        }
        public void Reload()
        {
            throw new System.NotImplementedException();
        }
    }
}
