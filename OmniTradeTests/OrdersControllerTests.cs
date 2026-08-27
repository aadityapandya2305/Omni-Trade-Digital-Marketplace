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
            var controller =
                new OrdersController(
                    mockOrderRepo.Object,
                    mockVendorRepo.Object);

            var claims = new List<Claim>();

            if (userId.HasValue)
            {
                claims.Add(
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        userId.Value.ToString()));
            }

            controller.ControllerContext =
                new ControllerContext
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
            var mockOrderRepo =
                new Mock<IOrderRepository>();

            var mockVendorRepo =
                new Mock<IVendorRepository>();

            var order = new OrderDetailsDto
            {
                Id = 1,
                CustomerId = 1,
                TotalAmount = 100,
                Status = "Pending"
            };

            mockOrderRepo
                .Setup(x => x.CreateOrderFromCartAsync(1))
                .ReturnsAsync(order);

            var controller =
                CreateController(
                    mockOrderRepo,
                    mockVendorRepo,
                    1);

            var result =
                await controller.Checkout(1);

            var okResult =
                Assert.IsType<OkObjectResult>(result);

            var returnedOrder =
                Assert.IsType<OrderDetailsDto>(okResult.Value);

            Assert.Equal(1, returnedOrder.Id);
            Assert.Equal(1, returnedOrder.CustomerId);
            Assert.Equal(100, returnedOrder.TotalAmount);
            Assert.Equal("Pending", returnedOrder.Status);
        }

        [Fact]
        public async Task Checkout_ReturnsUnauthorized_WhenNoUserClaim()
        {
            var mockOrderRepo =
                new Mock<IOrderRepository>();

            var mockVendorRepo =
                new Mock<IVendorRepository>();

            var controller =
                CreateController(
                    mockOrderRepo,
                    mockVendorRepo);

            var result =
                await controller.Checkout(1);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Checkout_ReturnsForbid_WhenCustomerIdDoesNotMatch()
        {
            var mockOrderRepo =
                new Mock<IOrderRepository>();

            var mockVendorRepo =
                new Mock<IVendorRepository>();

            var controller =
                CreateController(
                    mockOrderRepo,
                    mockVendorRepo,
                    1);

            var result =
                await controller.Checkout(2);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Checkout_ReturnsBadRequest_WhenCartOperationFails()
        {
            var mockOrderRepo =
                new Mock<IOrderRepository>();

            var mockVendorRepo =
                new Mock<IVendorRepository>();

            mockOrderRepo
                .Setup(x => x.CreateOrderFromCartAsync(1))
                .ThrowsAsync(
                    new InvalidOperationException(
                        "Cart is empty."));

            var controller =
                CreateController(
                    mockOrderRepo,
                    mockVendorRepo,
                    1);

            var result =
                await controller.Checkout(1);

            var badRequest =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Cart is empty.",
                badRequest.Value);
        }

        [Fact]
        public async Task GetCustomerOrders_ReturnsOk_WhenCustomerOwnsAccount()
        {
            var mockOrderRepo =
                new Mock<IOrderRepository>();

            var mockVendorRepo =
                new Mock<IVendorRepository>();

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

            var controller =
                CreateController(
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

            Assert.Contains(
                returnedOrders,
                o => o.Id == 1);

            Assert.Contains(
                returnedOrders,
                o => o.Id == 2);
        }

        [Fact]
        public async Task GetCustomerOrders_ReturnsUnauthorized_WhenNoUserClaim()
        {
            var mockOrderRepo =
                new Mock<IOrderRepository>();

            var mockVendorRepo =
                new Mock<IVendorRepository>();

            var controller =
                CreateController(
                    mockOrderRepo,
                    mockVendorRepo);

            var result =
                await controller.GetCustomerOrders(1);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetCustomerOrders_ReturnsForbid_WhenCustomerIdDoesNotMatch()
        {
            var mockOrderRepo =
                new Mock<IOrderRepository>();

            var mockVendorRepo =
                new Mock<IVendorRepository>();

            var controller =
                CreateController(
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
            var mockOrderRepo =
                new Mock<IOrderRepository>();

            var mockVendorRepo =
                new Mock<IVendorRepository>();

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
        },
        new OrderItem
        {
            Id = 2,
            OrderId = 2,
            VendorId = 1,
            Quantity = 1,
            UnitPrice = 100
        }
    };

            mockVendorRepo
                .Setup(x => x.GetVendorByUserIdAsync(1))
                .ReturnsAsync(vendor);

            mockOrderRepo
                .Setup(x => x.GetOrderItemsByVendorIdAsync(1))
                .ReturnsAsync(orderItems);

            var controller =
                CreateController(
                    mockOrderRepo,
                    mockVendorRepo,
                    1);

            var result =
                await controller.GetVendorOrders(1);

            var okResult =
                Assert.IsType<OkObjectResult>(result);

            var returnedItems =
                Assert.IsAssignableFrom<IEnumerable<OrderItem>>(
                    okResult.Value);

            Assert.Equal(2, returnedItems.Count());

            Assert.Contains(
                returnedItems,
                item => item.Id == 1);

            Assert.Contains(
                returnedItems,
                item => item.Id == 2);
        }

        [Fact]
        public async Task GetVendorOrders_ReturnsUnauthorized_WhenNoUserClaim()
        {
            var mockOrderRepo =
                new Mock<IOrderRepository>();

            var mockVendorRepo =
                new Mock<IVendorRepository>();

            var controller =
                CreateController(
                    mockOrderRepo,
                    mockVendorRepo);

            var result =
                await controller.GetVendorOrders(1);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetVendorOrders_ReturnsUnauthorized_WhenVendorProfileNotFound()
        {
            var mockOrderRepo =
                new Mock<IOrderRepository>();

            var mockVendorRepo =
                new Mock<IVendorRepository>();

            mockVendorRepo
                .Setup(x => x.GetVendorByUserIdAsync(1))
                .ReturnsAsync((Vendor?)null);

            var controller =
                CreateController(
                    mockOrderRepo,
                    mockVendorRepo,
                    1);

            var result =
                await controller.GetVendorOrders(1);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetVendorOrders_ReturnsForbid_WhenVendorIdDoesNotMatch()
        {
            var mockOrderRepo =
                new Mock<IOrderRepository>();

            var mockVendorRepo =
                new Mock<IVendorRepository>();

            var vendor = new Vendor
            {
                Id = 1,
                UserId = 1,
                StoreName = "Test Store",
                IsApproved = true
            };

            mockVendorRepo
                .Setup(x => x.GetVendorByUserIdAsync(1))
                .ReturnsAsync(vendor);

            var controller =
                CreateController(
                    mockOrderRepo,
                    mockVendorRepo,
                    1);

            var result =
                await controller.GetVendorOrders(2);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task UpdateOrderStatus_ReturnsUnauthorized_WhenNoUserClaim()
        {
            var mockOrderRepo =
                new Mock<IOrderRepository>();

            var mockVendorRepo =
                new Mock<IVendorRepository>();

            var controller =
                CreateController(
                    mockOrderRepo,
                    mockVendorRepo);

            var result =
                await controller.UpdateOrderStatus(
                    1,
                    "Processing");

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UpdateOrderStatus_ReturnsUnauthorized_WhenVendorProfileNotFound()
        {
            var mockOrderRepo =
                new Mock<IOrderRepository>();

            var mockVendorRepo =
                new Mock<IVendorRepository>();

            mockVendorRepo
                .Setup(x => x.GetVendorByUserIdAsync(1))
                .ReturnsAsync((Vendor?)null);

            var controller =
                CreateController(
                    mockOrderRepo,
                    mockVendorRepo,
                    1);

            var result =
                await controller.UpdateOrderStatus(
                    1,
                    "Processing");

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UpdateOrderStatus_ReturnsForbid_WhenVendorDoesNotOwnOrder()
        {
            var mockOrderRepo =
                new Mock<IOrderRepository>();

            var mockVendorRepo =
                new Mock<IVendorRepository>();

            var vendor = new Vendor
            {
                Id = 1,
                UserId = 1,
                StoreName = "Test Store",
                IsApproved = true
            };

            mockVendorRepo
                .Setup(x => x.GetVendorByUserIdAsync(1))
                .ReturnsAsync(vendor);

            mockOrderRepo
                .Setup(x => x.VendorHasOrderAsync(1, 1))
                .ReturnsAsync(false);

            var controller =
                CreateController(
                    mockOrderRepo,
                    mockVendorRepo,
                    1);

            var result =
                await controller.UpdateOrderStatus(
                    1,
                    "Processing");

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task UpdateOrderStatus_ReturnsOk_WhenVendorOwnsOrder()
        {
            var mockOrderRepo =
                new Mock<IOrderRepository>();

            var mockVendorRepo =
                new Mock<IVendorRepository>();

            var vendor = new Vendor
            {
                Id = 1,
                UserId = 1,
                StoreName = "Test Store",
                IsApproved = true
            };

            mockVendorRepo
                .Setup(x => x.GetVendorByUserIdAsync(1))
                .ReturnsAsync(vendor);

            mockOrderRepo
                .Setup(x => x.VendorHasOrderAsync(1, 1))
                .ReturnsAsync(true);

            mockOrderRepo
                .Setup(x => x.UpdateOrderStatusAsync(1, "Processing"))
                .Returns(Task.CompletedTask);

            var controller =
                CreateController(
                    mockOrderRepo,
                    mockVendorRepo,
                    1);

            var result =
                await controller.UpdateOrderStatus(
                    1,
                    "Processing");

            var okResult =
                Assert.IsType<OkObjectResult>(result);

            var message = okResult.Value?
                .GetType()
                .GetProperty("message")?
                .GetValue(okResult.Value)?
                .ToString();

            Assert.Equal(
                "Order status updated successfully.",
                message);

            mockOrderRepo.Verify(
                x => x.UpdateOrderStatusAsync(
                    1,
                    "Processing"),
                Times.Once);
        }

        [Fact]
        public async Task UpdateOrderStatus_ReturnsBadRequest_WhenStatusUpdateFails()
        {
            var mockOrderRepo =
                new Mock<IOrderRepository>();

            var mockVendorRepo =
                new Mock<IVendorRepository>();

            var vendor = new Vendor
            {
                Id = 1,
                UserId = 1,
                StoreName = "Test Store",
                IsApproved = true
            };

            mockVendorRepo
                .Setup(x => x.GetVendorByUserIdAsync(1))
                .ReturnsAsync(vendor);

            mockOrderRepo
                .Setup(x => x.VendorHasOrderAsync(1, 1))
                .ReturnsAsync(true);

            mockOrderRepo
                .Setup(x => x.UpdateOrderStatusAsync(1, "Delivered"))
                .ThrowsAsync(
                    new InvalidOperationException(
                        "Cannot change order status from 'Pending' to 'Delivered'."));

            var controller =
                CreateController(
                    mockOrderRepo,
                    mockVendorRepo,
                    1);

            var result =
                await controller.UpdateOrderStatus(
                    1,
                    "Delivered");

            var badRequest =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Cannot change order status from 'Pending' to 'Delivered'.",
                badRequest.Value);
        }

        [Fact]
        public async Task GetOrderById_ReturnsOk_WhenCustomerOwnsOrder()
        {
            var mockOrderRepo =
                new Mock<IOrderRepository>();

            var mockVendorRepo =
                new Mock<IVendorRepository>();

            var order = new OrderDetailsDto
            {
                Id = 5,
                CustomerId = 1,
                TotalAmount = 250,
                Status = "Pending"
            };

            mockOrderRepo
                .Setup(x => x.GetOrderDetailsForCustomerAsync(5, 1))
                .ReturnsAsync(order);

            var controller =
                CreateController(
                    mockOrderRepo,
                    mockVendorRepo,
                    1);

            var result =
                await controller.GetOrderById(5);

            var okResult =
                Assert.IsType<OkObjectResult>(result);

            var returnedOrder =
                Assert.IsType<OrderDetailsDto>(okResult.Value);

            Assert.Equal(5, returnedOrder.Id);
            Assert.Equal(1, returnedOrder.CustomerId);
        }

        [Fact]
        public async Task GetOrderById_ReturnsNotFound_WhenOrderDoesNotBelongToCustomer()
        {
            var mockOrderRepo =
                new Mock<IOrderRepository>();

            var mockVendorRepo =
                new Mock<IVendorRepository>();

            mockOrderRepo
                .Setup(x => x.GetOrderDetailsForCustomerAsync(5, 2))
                .ReturnsAsync((OrderDetailsDto?)null);

            var controller =
                CreateController(
                    mockOrderRepo,
                    mockVendorRepo,
                    2);

            var result =
                await controller.GetOrderById(5);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetOrderById_ReturnsUnauthorized_WhenNoUserClaim()
        {
            var mockOrderRepo =
                new Mock<IOrderRepository>();

            var mockVendorRepo =
                new Mock<IVendorRepository>();

            var controller =
                CreateController(
                    mockOrderRepo,
                    mockVendorRepo);

            var result =
                await controller.GetOrderById(5);

            Assert.IsType<UnauthorizedResult>(result);
        }
    }
}