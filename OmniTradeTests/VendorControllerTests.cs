using Moq;
using OmniTradeWebApi.Controllers;
using OmniTradeWebApi.Models;
using OmniTradeWebApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Xunit;

namespace OmniTradeTests
{
    public class VendorControllerTests
    {
        [Fact]
        public async Task GetVendor_ReturnsOk_WhenVendorExists()
        {
            var mockRepo = new Mock<IVendorRepository>();
            mockRepo.Setup(x => x.GetVendorByIdAsync(1)).ReturnsAsync(new Vendor { Id = 1, UserId = 1, StoreName = "Test Store" });

            var controller = new VendorsController(mockRepo.Object, null!);
            var result = await controller.GetVendor(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var vendor = okResult.Value as Vendor;
            Assert.NotNull(vendor);
            Assert.Equal(1, vendor.Id);
        }

        [Fact]
        public async Task GetVendor_ReturnsNotFound_WhenVendorDoesNotExist()
        {
            var mockRepo = new Mock<IVendorRepository>();
            mockRepo.Setup(x => x.GetVendorByIdAsync(99)).ReturnsAsync(null as Vendor?);

            var controller = new VendorsController(mockRepo.Object, null!);
            var result = await controller.GetVendor(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task RegisterVendor_ReturnsCreated_WhenSuccessful()
        {
            var mockRepo = new Mock<IVendorRepository>();
            mockRepo.Setup(x => x.GetVendorByUserIdAsync(1)).ReturnsAsync(null as Vendor?);

            var vendor = new Vendor { StoreName = "Test Store", Description = "A test store", ContactEmail = "test@test.com" };

            var controller = new VendorsController(mockRepo.Object, null!);
            var result = await controller.RegisterVendor(vendor);

            var createdResult = Assert.IsType<CreatedResult>(result.Result);
            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task RegisterVendor_ReturnsBadRequest_WhenAlreadyExists()
        {
            var mockRepo = new Mock<IVendorRepository>();
            var existingVendor = new Vendor { UserId = 1, StoreName = "Existing" };
            mockRepo.Setup(x => x.GetVendorByUserIdAsync(1)).ReturnsAsync(existingVendor);

            var vendor = new Vendor { StoreName = "Test Store" };

            var controller = new VendorsController(mockRepo.Object, null!);
            var result = await controller.RegisterVendor(vendor);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task ApproveVendor_ReturnsOk_WhenAdmin()
        {
            var mockRepo = new Mock<IVendorRepository>();
            mockRepo.Setup(x => x.GetVendorByIdAsync(1)).ReturnsAsync(new Vendor { Id = 1, IsApproved = false });

            var controller = new VendorsController(mockRepo.Object, null!);
            var result = await controller.ApproveVendor(1, true);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal("Vendor approved successfully.", okResult.Value?.ToString());
        }

        [Fact]
        public async Task GetVendorDashboard_ReturnsOk_WhenVendorAuthenticated()
        {
            var mockVendorRepo = new Mock<IVendorRepository>();
            mockVendorRepo.Setup(x => x.GetVendorByUserIdAsync(1)).ReturnsAsync(new Vendor { Id = 1, UserId = 1, StoreName = "Test Store", IsApproved = true });

            var mockProductRepo = new Mock<IProductRepository>();
            mockProductRepo.Setup(x => x.GetAllProductsAsync()).ReturnsAsync(new List<Product>
            {
                new Product { Id = 1, VendorId = 1, Name = "Product 1", IsActive = true, Price = 10, StockQuantity = 5 },
                new Product { Id = 2, VendorId = 1, Name = "Product 2", IsActive = false, Price = 20, StockQuantity = 10 }
            });

            var userId = "1";
            var authDefault = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }));
            var controller = new VendorsController(mockVendorRepo.Object, mockProductRepo.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = authDefault }
                }
            };

            var result = await controller.GetVendorDashboard();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dashboard = okResult.Value as dynamic;
            Assert.Equal(1, dashboard.vendorId);
            Assert.Equal("Test Store", dashboard.storeName);
            Assert.Equal(2, dashboard.totalProducts);
            Assert.Equal(1, dashboard.approvedProducts);
            Assert.Equal(1, dashboard.pendingProducts);
            Assert.Equal(15, dashboard.totalStock);
            Assert.True(dashboard.isApproved);
        }

        [Fact]
        public async Task UpdateVendorProfile_ReturnsOk_WhenSuccessful()
        {
            var mockRepo = new Mock<IVendorRepository>();
            var existingVendor = new Vendor { Id = 1, UserId = 1, StoreName = "Old Name", Description = "Old Desc", ContactEmail = "old@test.com" };
            mockRepo.Setup(x => x.GetVendorByUserIdAsync(1)).ReturnsAsync(existingVendor);
            mockRepo.Setup(x => x.UpdateVendorProfileAsync(It.IsAny<Vendor>())).Returns(Task.CompletedTask);

            var vendor = new Vendor { StoreName = "New Name", Description = "New Desc", ContactEmail = "new@test.com" };

            var userId = "1";
            var authDefault = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }));
            var controller = new VendorsController(mockRepo.Object, null!)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = authDefault }
                }
            };

            var result = await controller.UpdateVendorProfile(vendor);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal("Vendor profile updated successfully.", okResult.Value?.ToString());
            Assert.Equal("New Name", existingVendor.StoreName);
        }
    }
}