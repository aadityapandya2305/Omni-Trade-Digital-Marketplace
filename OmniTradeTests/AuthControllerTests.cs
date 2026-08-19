using Moq;
using OmniTradeWebApi.Controllers;
using OmniTradeWebApi.Models;
using OmniTradeWebApi.DTOs;
using OmniTradeWebApi.Services;
using OmniTradeWebApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace OmniTradeTests
{
    public class AuthControllerTests
    {
        [Fact]
        public async Task Register_ReturnsCreatedResult_WhenRegistrationIsSuccessful()
        {
            var mockAuthService = new Mock<IAuthService>();
            var registerDto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "password",
                AccountType = "Customer"
            };

            var expectedResponse = new RegisterResponseDto();

            mockAuthService.Setup(x => x.RegisterAsync(registerDto)).ReturnsAsync(expectedResponse);
            var controller = new AuthController(mockAuthService.Object);

            var result = await controller.Register(registerDto);

            var createdResult = Assert.IsType<CreatedResult>(result);
            Assert.Equal(expectedResponse, createdResult.Value);
        }

        [Fact]
        public async Task Login_ReturnsOkResult_WhenCredentialsAreValid()
        {
            var mockAuthService = new Mock<IAuthService>();
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "password"
            };

            var expectedResponse = new AuthResponseDto();

            mockAuthService.Setup(x => x.LoginAsync(loginDto)).ReturnsAsync(expectedResponse);
            var controller = new AuthController(mockAuthService.Object);

            var result = await controller.Login(loginDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }
    }
}
