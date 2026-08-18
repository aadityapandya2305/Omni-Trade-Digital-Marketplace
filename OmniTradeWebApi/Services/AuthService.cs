using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OmniTradeWebApi.DTOs;
using OmniTradeWebApi.Models;
using OmniTradeWebApi.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OmniTradeWebApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            PasswordHasher<User> passwordHasher,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        public async Task<RegisterResponseDto> RegisterAsync(RegisterDto request)
        {
            var existingUsername =
                await _userRepository.UsernameExistsAsync(request.Username);

            if (existingUsername)
            {
                throw new InvalidOperationException(
                    "Username is already registered.");
            }

            var existingEmail =
                await _userRepository.EmailExistsAsync(request.Email);

            if (existingEmail)
            {
                throw new InvalidOperationException(
                    "Email is already registered.");
            }

            string role;

            if (request.AccountType == "Customer")
            {
                role = "Customer";
            }
            else if (request.AccountType == "Vendor")
            {
                role = "Vendor";
            }
            else
            {
                throw new InvalidOperationException(
                    "Account type must be either Customer or Vendor.");
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Role = role
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                request.Password);

            await _userRepository.AddUserAsync(user);

            return new RegisterResponseDto
            {
                Message = "User registered successfully.",
                UserId = user.Id
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto request)
        {
            var user =
                await _userRepository.GetUserByEmailAsync(request.Email);

            if (user == null)
            {
                throw new InvalidOperationException(
                    "Invalid email or password.");
            }

            var passwordResult =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    request.Password);

            if (passwordResult == PasswordVerificationResult.Failed)
            {
                throw new InvalidOperationException(
                    "Invalid email or password.");
            }

            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }

        private string GenerateJwtToken(User user)
        {
            var key = _configuration["Jwt:Key"];

            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException(
                    "JWT signing key is not configured.");
            }

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    user.Username),

                new Claim(
                    ClaimTypes.Email,
                    user.Email),

                new Claim(
                    ClaimTypes.Role,
                    user.Role)
            };

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    double.Parse(
                        _configuration["Jwt:ExpirationMinutes"]!)),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}