using Moq;
using OmniTradeWebApi.Controllers;
using OmniTradeWebApi.Models;
using OmniTradeWebApi.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace OmniTradeTests
{
    public class VendorControllerTests
    {
        [Fact]
        public async Task GetVendor_ReturnsOk_WhenVendorExists()
        {
            var mockVendorRepo = new Mock<IVendorRepository>();
            var mockProductRepo = new Mock<IProductRepository>();

            mockVendorRepo
                .Setup(x => x.GetVendorByIdAsync(1))
                .ReturnsAsync(new Vendor
                {
                    Id = 1,
                    UserId = 1,
                    StoreName = "Test Store"
                });

            var controller = new VendorsController(
                mockVendorRepo.Object,
                mockProductRepo.Object);

            var result = await controller.GetVendor(1);

            var okResult =
                Assert.IsType<OkObjectResult>(result.Result);

            var vendor =
                Assert.IsType<Vendor>(okResult.Value);

            Assert.Equal(1, vendor.Id);
        }

        [Fact]
        public async Task GetVendor_ReturnsNotFound_WhenVendorDoesNotExist()
        {
            var mockVendorRepo = new Mock<IVendorRepository>();
            var mockProductRepo = new Mock<IProductRepository>();

            mockVendorRepo
                .Setup(x => x.GetVendorByIdAsync(99))
                .ReturnsAsync((Vendor?)null);

            var controller = new VendorsController(
                mockVendorRepo.Object,
                mockProductRepo.Object);

            var result = await controller.GetVendor(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task RegisterVendor_ReturnsCreated_WhenSuccessful()
        {
            var mockVendorRepo = new Mock<IVendorRepository>();
            var mockProductRepo = new Mock<IProductRepository>();

            mockVendorRepo
                .Setup(x => x.GetVendorByUserIdAsync(1))
                .ReturnsAsync((Vendor?)null);

            var vendor = new Vendor
            {
                StoreName = "Test Store",
                Description = "A test store",
                ContactEmail = "test@test.com"
            };

            var controller = new VendorsController(
                mockVendorRepo.Object,
                mockProductRepo.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new[]
                            {
                                new Claim(
                                    ClaimTypes.NameIdentifier,
                                    "1")
                            }))
                }
            };

            var result =
                await controller.RegisterVendor(vendor);

            var createdResult =
                Assert.IsType<ObjectResult>(result);

            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task RegisterVendor_ReturnsBadRequest_WhenAlreadyExists()
        {
            var mockVendorRepo = new Mock<IVendorRepository>();
            var mockProductRepo = new Mock<IProductRepository>();

            mockVendorRepo
                .Setup(x => x.GetVendorByUserIdAsync(1))
                .ReturnsAsync(new Vendor
                {
                    UserId = 1,
                    StoreName = "Existing"
                });

            var vendor = new Vendor
            {
                StoreName = "Test Store"
            };

            var controller = new VendorsController(
                mockVendorRepo.Object,
                mockProductRepo.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new[]
                            {
                                new Claim(
                                    ClaimTypes.NameIdentifier,
                                    "1")
                            }))
                }
            };

            var result =
                await controller.RegisterVendor(vendor);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ApproveVendor_ReturnsOk_WhenAdmin()
        {
            var mockVendorRepo = new Mock<IVendorRepository>();
            var mockProductRepo = new Mock<IProductRepository>();

            mockVendorRepo
                .Setup(x => x.GetVendorByIdAsync(1))
                .ReturnsAsync(new Vendor
                {
                    Id = 1,
                    IsApproved = false
                });

            var controller = new VendorsController(
                mockVendorRepo.Object,
                mockProductRepo.Object);

            var result =
                await controller.ApproveVendor(1, true);

            var okResult =
                Assert.IsType<OkObjectResult>(result);

            var message = okResult.Value?
                .GetType()
                .GetProperty("message")?
                .GetValue(okResult.Value)?
                .ToString();

            Assert.Equal(
                "Vendor approved successfully.",
                message);
        }

        [Fact]
        public async Task GetVendorDashboard_ReturnsOk_WhenVendorAuthenticated()
        {
            var mockVendorRepo = new Mock<IVendorRepository>();

            mockVendorRepo
                .Setup(x => x.GetVendorByUserIdAsync(1))
                .ReturnsAsync(new Vendor
                {
                    Id = 1,
                    UserId = 1,
                    StoreName = "Test Store",
                    IsApproved = true
                });

            var mockProductRepo = new Mock<IProductRepository>();

            mockProductRepo
                .Setup(x => x.GetAllProductsAsync())
                .ReturnsAsync(new List<Product>
                {
                    new Product
                    {
                        Id = 1,
                        VendorId = 1,
                        Name = "Product 1",
                        Price = 10,
                        StockQuantity = 5
                    },
                    new Product
                    {
                        Id = 2,
                        VendorId = 1,
                        Name = "Product 2",
                        Price = 20,
                        StockQuantity = 10
                    }
                });

            var authDefault =
                new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                            new Claim(
                                ClaimTypes.NameIdentifier,
                                "1")
                        }));

            var controller = new VendorsController(
                mockVendorRepo.Object,
                mockProductRepo.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = authDefault
                    }
                }
            };

            var result =
                await controller.GetVendorDashboard();

            var okResult =
                Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task UpdateVendorProfile_ReturnsOk_WhenSuccessful()
        {
            var mockVendorRepo = new Mock<IVendorRepository>();
            var mockProductRepo = new Mock<IProductRepository>();

            var existingVendor = new Vendor
            {
                Id = 1,
                UserId = 1,
                StoreName = "Old Name",
                Description = "Old Desc",
                ContactEmail = "old@test.com"
            };

            mockVendorRepo
                .Setup(x => x.GetVendorByUserIdAsync(1))
                .ReturnsAsync(existingVendor);

            mockVendorRepo
                .Setup(x => x.UpdateVendorProfileAsync(
                    It.IsAny<Vendor>()))
                .Returns(Task.CompletedTask);

            var vendor = new Vendor
            {
                StoreName = "New Name",
                Description = "New Desc",
                ContactEmail = "new@test.com"
            };

            var authDefault =
                new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                            new Claim(
                                ClaimTypes.NameIdentifier,
                                "1")
                        }));

            var controller = new VendorsController(
                mockVendorRepo.Object,
                mockProductRepo.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = authDefault
                    }
                }
            };

            var result =
                await controller.UpdateVendorProfile(vendor);

            var okResult =
                Assert.IsType<OkObjectResult>(result);

            var message = okResult.Value?
                .GetType()
                .GetProperty("message")?
                .GetValue(okResult.Value)?
                .ToString();

            Assert.Equal(
                "Vendor profile updated successfully.",
                message);

            Assert.Equal(
                "New Name",
                existingVendor.StoreName);
        }
    }
}