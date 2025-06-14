
using Microsoft.AspNetCore.Mvc;
using NetCoreAudio;
using NetCoreAudio.Interfaces;
using PlayerAPI.Services;

namespace PlayerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        private IMyPlayer player;

        private IPlaylistProvider playlistProvider;

        public PlayerController(IMyPlayer player, IPlaylistProvider playlistProvider)
        {
            this.player = player;
            this.playlistProvider = playlistProvider;
        }

        [HttpGet("Play/{id:min(0)}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult Play(int id)
        {
            if (!playlistProvider.TryGetPLaylist(id, out var playList) || playList == null)
            {
                return NotFound();
            }

            player.Play(playList);

            return Ok();
        }

        [HttpGet("Pause")]
        public void Pause()
        {
            player.Pause();
        }

        [HttpGet("Resume")]
        public void Resume()
        {
            player.Resume();
        }

        [HttpGet("Next")]
        public void Next()
        {
            player.Next();
        }

        [HttpGet("Previous")]
        public void Previous()
        {
            player.Previous();
        }

    }
}
