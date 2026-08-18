using OmniTradeWebApi.Models;

namespace OmniTradeWebApi.Repositories
{
    public interface IUserRepository
    {
        Task<bool> UsernameExistsAsync(string username);

        Task<bool> EmailExistsAsync(string email);

        Task<User?> GetUserByEmailAsync(string email);

        Task AddUserAsync(User user);
    }
}