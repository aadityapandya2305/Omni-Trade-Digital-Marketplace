using OmniTradeWebApi.DTOs;

namespace OmniTradeWebApi.Services
{
    public interface IAuthService
    {
        Task<RegisterResponseDto> RegisterAsync(RegisterDto request);

        Task<AuthResponseDto> LoginAsync(LoginDto request);
    }
}