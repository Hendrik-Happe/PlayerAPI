using Microsoft.AspNetCore.Mvc;
using PlayerAPI.Models;
using PlayerAPI.Services;

namespace PlayerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaylistController(IPlaylistProvider playlistProvider) : ControllerBase
    {
        private readonly IPlaylistProvider playlistProvider = playlistProvider;

        [HttpGet("Index")]
        [ProducesResponseType<List<PlayList>>(StatusCodes.Status200OK)]
        public ActionResult<List<PlayList>> Index()
        {
            return playlistProvider.GetAllPlaylists();
        }

        [HttpGet("{id}")]
        [ProducesResponseType<PlayList>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<PlayList?> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            if (!playlistProvider.TryGetPLaylist(id.Value, out var playlist))
            {
                return NotFound();
            }

            return playlist;
        }

        [HttpGet("/Reload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public void Reload()
        {
            playlistProvider.Reload();
        }
    }
}