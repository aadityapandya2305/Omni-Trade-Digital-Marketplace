using Microsoft.EntityFrameworkCore;
using OmniTradeWebApi.Data;
using OmniTradeWebApi.Models;

namespace OmniTradeWebApi.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly OmniTradeHubContext _context;

        public UserRepository(OmniTradeHubContext context)
        {
            _context = context;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
    }
}