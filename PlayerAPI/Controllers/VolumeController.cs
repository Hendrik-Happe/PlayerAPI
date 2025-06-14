using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlayerAPI.Services;

namespace PlayerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VolumeController : ControllerBase
    {
        private IVolumeHandler volumeHandler;

        public VolumeController(IVolumeHandler volumeHandler)
        {
            this.volumeHandler = volumeHandler;
        }

        [HttpPost("VolumeUp")]
        public void VolumeUp()
        {
            volumeHandler.IncreaseVolume();
        }
        
        [HttpPost("VolumeDown")]
        public void VolumeDown()
        {
            volumeHandler.DecreaseVolume();
        }

        [HttpGet("CurrentVolume")]
        public ActionResult<byte> GetCurrentVolume()
        {
            return volumeHandler.GetVolume();
        }
    }
}
