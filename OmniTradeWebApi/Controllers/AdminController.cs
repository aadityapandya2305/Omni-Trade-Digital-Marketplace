using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniTradeWebApi.Repositories;

namespace OmniTradeWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminRepository _adminRepository;

        public AdminController(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        [HttpGet("analytics")]
        public async Task<ActionResult> GetPlatformAnalytics()
        {
            var analytics =
                await _adminRepository.GetPlatformAnalyticsAsync();

            return Ok(analytics);
        }

        [HttpGet("users")]
        public async Task<ActionResult> GetAllUsers()
        {
            var users = await _adminRepository.GetAllUsersAsync();

            return Ok(users);
        }
    }
}