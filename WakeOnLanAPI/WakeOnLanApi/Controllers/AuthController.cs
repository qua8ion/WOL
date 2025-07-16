using Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dto.Services.Auth;
using Services.Auth;

namespace WakeOnLanApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;

        public AuthController(ILogger<AuthController> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Login([FromBody] AuthModel model)
        {
            var result = await _authService.AuthAsync(model);

            if (result is null) return Unauthorized();

            return Ok(result);
        }

        [HttpGet]
        [Route("[action]")]
        [AllowAnonymous]
        public IActionResult ValidateToken([FromHeader(Name = "Authorization")] string bearerToken)
        {
            return Ok(_authService.ValidateJwtToken(bearerToken));
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult Register(string userName, string password)
        {
            return Ok();
        }
    }
}