using Microsoft.AspNetCore.Mvc;
using OmniTradeWebApi.DTOs;
using OmniTradeWebApi.Services;

namespace OmniTradeWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            var result = await _authService.RegisterAsync(request);

            return Created("", result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var result = await _authService.LoginAsync(request);

            return Ok(result);
        }
    }
}