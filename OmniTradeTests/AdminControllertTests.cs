using Moq;
using OmniTradeWebApi.Controllers;
using OmniTradeWebApi.DTOs;
using OmniTradeWebApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace OmniTradeTests
{
    public class AdminControllerTests
    {
        [Fact]
        public async Task GetPlatformAnalytics_ReturnsOk()
        {
            var mockRepo = new Mock<IAdminRepository>();

            var analytics = new PlatformAnalyticsDto
            {
                GMV = 17495,
                TotalActiveVendors = 1,
                TotalOrders = 1
            };

            mockRepo.Setup(x => x.GetPlatformAnalyticsAsync()).ReturnsAsync(analytics);

            var controller = new AdminController(mockRepo.Object);

            var result = await controller.GetPlatformAnalytics();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnedAnalytics = Assert.IsType<PlatformAnalyticsDto>(okResult.Value);

            Assert.Equal(17495, returnedAnalytics.GMV);
            Assert.Equal(1, returnedAnalytics.TotalActiveVendors);
            Assert.Equal(1, returnedAnalytics.TotalOrders);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsOk()
        {
            var mockRepo = new Mock<IAdminRepository>();

            var users = new List<UserManagementDto>
            {
                new UserManagementDto
                {
                    Id = 1,
                    Username = "DummyCustomer",
                    Email = "dummy@example.com",
                    Role = "Customer"
                },
                new UserManagementDto
                {
                    Id = 2,
                    Username = "DummyVendor",
                    Email = "vendor@example.com",
                    Role = "Vendor"
                },
                new UserManagementDto
                {
                    Id = 3,
                    Username = "admin",
                    Email = "admin@omnitradehub.com",
                    Role = "Admin"
                }
            };

            mockRepo.Setup(x => x.GetAllUsersAsync()).ReturnsAsync(users);

            var controller = new AdminController(mockRepo.Object);

            var result = await controller.GetAllUsers();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnedUsers = Assert.IsAssignableFrom<IEnumerable<UserManagementDto>>(okResult.Value);

            Assert.Equal(3, returnedUsers.Count());

            Assert.Contains(returnedUsers,u => u.Username == "admin" && u.Role == "Admin");
        }
    }
}