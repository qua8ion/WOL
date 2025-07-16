using Microsoft.AspNetCore.Mvc;

namespace WakeOnLanApi.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : Controller
{
    [HttpGet]
    [Route("[action]")]
    public IActionResult IsAlive()
    {
        return Ok("I' am alive!");
    }
}