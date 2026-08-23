using Moq;
using OmniTradeWebApi.Controllers;
using OmniTradeWebApi.Models;
using OmniTradeWebApi.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace OmniTradeTests
{
    public class ProductsControllerTests
    {
        [Fact]
        public async Task GetProducts_ReturnsOk()
        {
            var mockRepo = new Mock<IProductRepository>();

            mockRepo
                .Setup(x => x.GetAllProductsAsync())
                .ReturnsAsync(new List<Product>
                {
                    new Product
                    {
                        Id = 1,
                        Name = "Product 1",
                        Price = 10,
                        StockQuantity = 5
                    }
                });

            var controller = new ProductsController(
                mockRepo.Object,
                null!);

            var result = await controller.GetProducts();

            var okResult =
                Assert.IsType<OkObjectResult>(result.Result);

            var products =
                Assert.IsAssignableFrom<IEnumerable<Product>>(
                    okResult.Value);

            Assert.Single(products);
        }

        [Fact]
        public async Task GetProduct_ReturnsOk_WhenExists()
        {
            var mockRepo = new Mock<IProductRepository>();

            mockRepo
                .Setup(x => x.GetProductByIdAsync(1))
                .ReturnsAsync(new Product
                {
                    Id = 1,
                    Name = "Product 1"
                });

            var controller = new ProductsController(
                mockRepo.Object,
                null!);

            var result = await controller.GetProduct(1);

            var okResult =
                Assert.IsType<OkObjectResult>(result.Result);

            var product =
                Assert.IsType<Product>(okResult.Value);

            Assert.Equal(1, product.Id);
        }

        [Fact]
        public async Task GetProduct_ReturnsNotFound_WhenDoesNotExist()
        {
            var mockRepo = new Mock<IProductRepository>();

            mockRepo
                .Setup(x => x.GetProductByIdAsync(99))
                .ReturnsAsync((Product?)null);

            var controller = new ProductsController(
                mockRepo.Object,
                null!);

            var result = await controller.GetProduct(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task SearchProducts_ReturnsOk_WhenValidName()
        {
            var mockRepo = new Mock<IProductRepository>();

            mockRepo
                .Setup(x => x.GetProductsByNameAsync("Product"))
                .ReturnsAsync(new List<Product>
                {
                    new Product
                    {
                        Id = 1,
                        Name = "Product 1"
                    }
                });

            var controller = new ProductsController(
                mockRepo.Object,
                null!);

            var result =
                await controller.SearchProducts("Product");

            var okResult =
                Assert.IsType<OkObjectResult>(result.Result);

            var products =
                Assert.IsAssignableFrom<IEnumerable<Product>>(
                    okResult.Value);

            Assert.Single(products);
        }

        [Fact]
        public async Task SearchProducts_ReturnsBadRequest_WhenEmptyName()
        {
            var mockRepo = new Mock<IProductRepository>();

            var controller = new ProductsController(
                mockRepo.Object,
                null!);

            var result =
                await controller.SearchProducts("");

            Assert.IsType<BadRequestObjectResult>(
                result.Result);
        }

        [Fact]
        public async Task AddProduct_ReturnsCreated_WhenVendorApproved()
        {
            var mockProductRepo = new Mock<IProductRepository>();
            var mockVendorRepo = new Mock<IVendorRepository>();

            var vendor = new Vendor
            {
                Id = 1,
                UserId = 1,
                IsApproved = true,
                StoreName = "Test Store"
            };

            mockVendorRepo
                .Setup(x => x.GetVendorByUserIdAsync(1))
                .ReturnsAsync(vendor);

            mockProductRepo
                .Setup(x => x.AddProductAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            var controller = new ProductsController(
                mockProductRepo.Object,
                mockVendorRepo.Object)
            {
                ControllerContext = new ControllerContext
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
                }
            };

            var product = new Product
            {
                Name = "Test Product",
                Description = "Desc",
                Price = 25,
                StockQuantity = 10,
                Category = "Electronics"
            };

            var result =
                await controller.AddProduct(product);

            var createdResult =
                Assert.IsType<ObjectResult>(result);

            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task AddProduct_ReturnsForbid_WhenVendorNotApproved()
        {
            var mockProductRepo = new Mock<IProductRepository>();
            var mockVendorRepo = new Mock<IVendorRepository>();

            var vendor = new Vendor
            {
                Id = 1,
                UserId = 1,
                IsApproved = false,
                StoreName = "Test Store"
            };

            mockVendorRepo
                .Setup(x => x.GetVendorByUserIdAsync(1))
                .ReturnsAsync(vendor);

            var controller = new ProductsController(
                mockProductRepo.Object,
                mockVendorRepo.Object)
            {
                ControllerContext = new ControllerContext
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
                }
            };

            var product = new Product
            {
                Name = "Test Product",
                Price = 25,
                StockQuantity = 10,
                Category = "Electronics"
            };

            var result =
                await controller.AddProduct(product);

            var statusResult =
                Assert.IsType<ObjectResult>(result);

            Assert.Equal(403, statusResult.StatusCode);
        }

        [Fact]
        public async Task AddProduct_ReturnsBadRequest_WhenNoVendorProfile()
        {
            var mockProductRepo = new Mock<IProductRepository>();
            var mockVendorRepo = new Mock<IVendorRepository>();

            mockVendorRepo
                .Setup(x => x.GetVendorByUserIdAsync(1))
                .ReturnsAsync((Vendor?)null);

            var controller = new ProductsController(
                mockProductRepo.Object,
                mockVendorRepo.Object)
            {
                ControllerContext = new ControllerContext
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
                }
            };

            var product = new Product
            {
                Name = "Test Product",
                Price = 25,
                StockQuantity = 10,
                Category = "Electronics"
            };

            var result =
                await controller.AddProduct(product);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateProduct_ReturnsOk_WhenVendorOwnsProduct()
        {
            var mockProductRepo = new Mock<IProductRepository>();
            var mockVendorRepo = new Mock<IVendorRepository>();

            var vendor = new Vendor
            {
                Id = 1,
                UserId = 1,
                IsApproved = true,
                StoreName = "Test Store"
            };

            mockVendorRepo
                .Setup(x => x.GetVendorByUserIdAsync(1))
                .ReturnsAsync(vendor);

            var existingProduct = new Product
            {
                Id = 1,
                VendorId = 1,
                Name = "Old Name",
                Description = "Old Desc",
                Price = 10,
                StockQuantity = 5,
                Category = "Test"
            };

            mockProductRepo
                .Setup(x => x.GetProductByIdAsync(1))
                .ReturnsAsync(existingProduct);

            mockProductRepo
                .Setup(x => x.UpdateProductAsync(
                    It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            var controller = new ProductsController(
                mockProductRepo.Object,
                mockVendorRepo.Object)
            {
                ControllerContext = new ControllerContext
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
                }
            };

            var updatedProduct = new Product
            {
                Name = "New Name",
                Description = "New Desc",
                Price = 20,
                StockQuantity = 15,
                Category = "Test"
            };

            var result =
                await controller.UpdateProduct(
                    1,
                    updatedProduct);

            var okResult =
                Assert.IsType<OkObjectResult>(result);

            var message = okResult.Value?
                .GetType()
                .GetProperty("message")?
                .GetValue(okResult.Value)?
                .ToString();

            Assert.Equal(
                "Product updated successfully.",
                message);

            Assert.Equal(
                "New Name",
                existingProduct.Name);
        }

        [Fact]
        public async Task UpdateProduct_ReturnsForbid_WhenNotOwnProduct()
        {
            var mockProductRepo = new Mock<IProductRepository>();
            var mockVendorRepo = new Mock<IVendorRepository>();

            var vendor = new Vendor
            {
                Id = 1,
                UserId = 1,
                IsApproved = true,
                StoreName = "Test Store"
            };

            mockVendorRepo
                .Setup(x => x.GetVendorByUserIdAsync(1))
                .ReturnsAsync(vendor);

            var existingProduct = new Product
            {
                Id = 1,
                VendorId = 99,
                Name = "Other Vendor Product"
            };

            mockProductRepo
                .Setup(x => x.GetProductByIdAsync(1))
                .ReturnsAsync(existingProduct);

            var controller = new ProductsController(
                mockProductRepo.Object,
                mockVendorRepo.Object)
            {
                ControllerContext = new ControllerContext
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
                }
            };

            var result =
                await controller.UpdateProduct(
                    1,
                    new Product
                    {
                        Name = "New Name"
                    });

            var statusResult =
                Assert.IsType<ObjectResult>(result);

            Assert.Equal(403, statusResult.StatusCode);
        }

        [Fact]
        public async Task DeleteProduct_ReturnsOk_WhenVendorOwnsProduct()
        {
            var mockProductRepo = new Mock<IProductRepository>();
            var mockVendorRepo = new Mock<IVendorRepository>();

            var vendor = new Vendor
            {
                Id = 1,
                UserId = 1,
                IsApproved = true,
                StoreName = "Test Store"
            };

            mockVendorRepo
                .Setup(x => x.GetVendorByUserIdAsync(1))
                .ReturnsAsync(vendor);

            mockProductRepo
                .Setup(x => x.GetProductByIdAsync(1))
                .ReturnsAsync(new Product
                {
                    Id = 1,
                    VendorId = 1
                });

            mockProductRepo
                .Setup(x => x.DeleteProductAsync(1))
                .Returns(Task.CompletedTask);

            var controller = new ProductsController(
                mockProductRepo.Object,
                mockVendorRepo.Object)
            {
                ControllerContext = new ControllerContext
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
                }
            };

            var result =
                await controller.DeleteProduct(1);

            var okResult =
                Assert.IsType<OkObjectResult>(result);

            var message = okResult.Value?
                .GetType()
                .GetProperty("message")?
                .GetValue(okResult.Value)?
                .ToString();

            Assert.Equal(
                "Product deleted successfully.",
                message);
        }

        [Fact]
        public async Task DeleteProduct_ReturnsNotFound_WhenProductDoesNotExist()
        {
            var mockProductRepo = new Mock<IProductRepository>();
            var mockVendorRepo = new Mock<IVendorRepository>();

            var vendor = new Vendor
            {
                Id = 1,
                UserId = 1,
                IsApproved = true,
                StoreName = "Test Store"
            };

            mockVendorRepo
                .Setup(x => x.GetVendorByUserIdAsync(1))
                .ReturnsAsync(vendor);

            mockProductRepo
                .Setup(x => x.GetProductByIdAsync(99))
                .ReturnsAsync((Product?)null);

            var controller = new ProductsController(
                mockProductRepo.Object,
                mockVendorRepo.Object)
            {
                ControllerContext = new ControllerContext
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
                }
            };

            var result =
                await controller.DeleteProduct(99);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetProductsByFilter_ReturnsFilteredResults()
        {
            var mockRepo = new Mock<IProductRepository>();

            mockRepo
                .Setup(x => x.GetProductsByFilterAsync(
                    name: "Product",
                    category: null,
                    minPrice: null,
                    maxPrice: null,
                    vendorId: null))
                .ReturnsAsync(new List<Product>
                {
                    new Product
                    {
                        Id = 1,
                        Name = "Product 1"
                    }
                });

            var controller = new ProductsController(
                mockRepo.Object,
                null!);

            var result =
                await controller.GetProductsByFilter(
                    name: "Product");

            var okResult =
                Assert.IsType<OkObjectResult>(result.Result);

            var products =
                Assert.IsAssignableFrom<IEnumerable<Product>>(
                    okResult.Value);

            Assert.Single(products);
        }
    }
}