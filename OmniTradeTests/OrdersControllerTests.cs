using Moq;
using OmniTradeWebApi.Controllers;
using OmniTradeWebApi.DTOs;
using OmniTradeWebApi.Models;
using OmniTradeWebApi.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace OmniTradeTests
{
    public class OrdersControllerTests
    {
        private static OrdersController CreateController(
            Mock<IOrderRepository> mockOrderRepo,
            Mock<IVendorRepository> mockVendorRepo,
            int? userId = null)
        {
            var controller = new OrdersController(
                mockOrderRepo.Object,
                mockVendorRepo.Object);

            var claims = new List<Claim>();

            if (userId.HasValue)
            {
                claims.Add(new Claim(
                    ClaimTypes.NameIdentifier,
                    userId.Value.ToString()));
            }

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(claims))
                }
            };

            return controller;
        }

        [Fact]
        public async Task Checkout_ReturnsOk_WhenCustomerOwnsAccount()
        {
            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockVendorRepo = new Mock<IVendorRepository>();

            var order = new Order
            {
                Id = 1,
                CustomerId = 1,
                TotalAmount = 100,
                Status = "Pending",
                ShippingAddress = "123 Test Street",
                PaymentMethod = "COD"
            };

            mockOrderRepo
                .Setup(x => x.CreateOrderFromCartAsync(
                    1,
                    "123 Test Street",
                    "COD"))
                .ReturnsAsync(order);

            var controller = CreateController(
                mockOrderRepo,
                mockVendorRepo,
                1);

            var request = new OrderCheckoutDto
            {
                ShippingAddress = "123 Test Street",
                PaymentMethod = "COD"
            };

            var result = await controller.Checkout(1, request);

            var okResult =
                Assert.IsType<OkObjectResult>(result);

            var returnedOrder =
                Assert.IsType<Order>(okResult.Value);

            Assert.Equal(1, returnedOrder.Id);
            Assert.Equal(1, returnedOrder.CustomerId);
            Assert.Equal(100, returnedOrder.TotalAmount);
            Assert.Equal("Pending", returnedOrder.Status);
        }

        [Fact]
        public async Task Checkout_ReturnsUnauthorized_WhenNoUserClaim()
        {
            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockVendorRepo = new Mock<IVendorRepository>();

            var controller = CreateController(
                mockOrderRepo,
                mockVendorRepo);

            var request = new OrderCheckoutDto
            {
                ShippingAddress = "123 Test Street",
                PaymentMethod = "COD"
            };

            var result = await controller.Checkout(1, request);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Checkout_ReturnsForbid_WhenCustomerIdDoesNotMatch()
        {
            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockVendorRepo = new Mock<IVendorRepository>();

            var controller = CreateController(
                mockOrderRepo,
                mockVendorRepo,
                1);

            var request = new OrderCheckoutDto
            {
                ShippingAddress = "123 Test Street",
                PaymentMethod = "COD"
            };

            var result = await controller.Checkout(2, request);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Checkout_ReturnsBadRequest_WhenCartOperationFails()
        {
            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockVendorRepo = new Mock<IVendorRepository>();

            mockOrderRepo
                .Setup(x => x.CreateOrderFromCartAsync(
                    1,
                    "123 Test Street",
                    "COD"))
                .ThrowsAsync(
                    new InvalidOperationException(
                        "Cart is empty."));

            var controller = CreateController(
                mockOrderRepo,
                mockVendorRepo,
                1);

            var request = new OrderCheckoutDto
            {
                ShippingAddress = "123 Test Street",
                PaymentMethod = "COD"
            };

            var result = await controller.Checkout(1, request);

            var badRequest =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Cart is empty.",
                badRequest.Value);
        }

        [Fact]
        public async Task GetCustomerOrders_ReturnsOk_WhenCustomerOwnsAccount()
        {
            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockVendorRepo = new Mock<IVendorRepository>();

            var orders = new List<Order>
            {
                new Order
                {
                    Id = 1,
                    CustomerId = 1,
                    TotalAmount = 100,
                    Status = "Pending"
                },
                new Order
                {
                    Id = 2,
                    CustomerId = 1,
                    TotalAmount = 200,
                    Status = "Delivered"
                }
            };

            mockOrderRepo
                .Setup(x => x.GetOrdersByCustomerIdAsync(1))
                .ReturnsAsync(orders);

            var controller = CreateController(
                mockOrderRepo,
                mockVendorRepo,
                1);

            var result =
                await controller.GetCustomerOrders(1);

            var okResult =
                Assert.IsType<OkObjectResult>(result);

            var returnedOrders =
                Assert.IsAssignableFrom<IEnumerable<Order>>(
                    okResult.Value);

            Assert.Equal(2, returnedOrders.Count());
        }

        [Fact]
        public async Task GetCustomerOrders_ReturnsUnauthorized_WhenNoUserClaim()
        {
            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockVendorRepo = new Mock<IVendorRepository>();

            var controller = CreateController(
                mockOrderRepo,
                mockVendorRepo);

            var result =
                await controller.GetCustomerOrders(1);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetCustomerOrders_ReturnsForbid_WhenCustomerIdDoesNotMatch()
        {
            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockVendorRepo = new Mock<IVendorRepository>();

            var controller = CreateController(
                mockOrderRepo,
                mockVendorRepo,
                1);

            var result =
                await controller.GetCustomerOrders(2);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task GetVendorOrders_ReturnsOk_WhenVendorOwnsAccount()
        {
            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockVendorRepo = new Mock<IVendorRepository>();

            var vendor = new Vendor
            {
                Id = 1,
                UserId = 1,
                StoreName = "Test Store",
                IsApproved = true
            };

            var orderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = 1,
                    OrderId = 1,
                    VendorId = 1,
                    Quantity = 2,
                    UnitPrice = 50
                }
            };

            mockVendorRepo
                .Setup(x => x.GetVendorByUserIdAsync(1))
                .ReturnsAsync(vendor);

            mockOrderRepo
                .Setup(x => x.GetOrderItemsByVendorIdAsync(1))
                .ReturnsAsync(orderItems);

            var controller = CreateController(
                mockOrderRepo,
                mockVendorRepo,
                1);

            var result =
                await controller.GetVendorOrders(1);

            var okResult =
                Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(okResult.Value);
        }
    }
}