using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Services.BusinessLogic.NetworkManager;
using WOLLib;
using Dto.Services;

namespace WakeOnLanApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class NetworkManagerController : ControllerBase
    {

        private readonly ILogger<NetworkManagerController> _logger;
        private readonly INetworkManagerService _srv;

        public NetworkManagerController(ILogger<NetworkManagerController> logger, INetworkManagerService srv)
        {
            _logger = logger;
            _srv = srv;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetDevices()
        {
            return Ok(await _srv.GetDevicesAsync());
        }

        [HttpPost]
        [Route("[action]")]        
        public async Task<IActionResult> PingDevices(string[] ipCollection)
        {
            return Ok(await _srv.PingDevicesAsync(ipCollection));
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> SendMagicPacket(DeviceModel model)
        {
            //var user = User.FindFirst(ClaimTypes.Name)?.Value;
            return Ok(await _srv.SendMagicPacket(model));
        }
    }
}