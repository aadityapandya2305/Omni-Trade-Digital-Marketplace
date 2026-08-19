using Moq;
using OmniTradeWebApi.Controllers;
using OmniTradeWebApi.Models;
using OmniTradeWebApi.DTOs;
using OmniTradeWebApi.Services;
using OmniTradeWebApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace OmniTradeTests
{
    public class CartControllerTests
    {
        private List<CartItem> sampleCartItems = new List<CartItem>
        {
            new CartItem{Id = 1, CustomerId = 1, ProductId = 101, Quantity = 2},
            new CartItem{Id = 2, CustomerId = 1, ProductId = 102, Quantity = 1}
        };

        private void SetUserContext(ControllerBase controller, String userId)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, "Customer")
            }, "TestAuth"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetCart_ReturnsOkResult_WhenUserMatchesCustomerId()
        {
            var mockRepo = new Mock<ICartRepository>();
            mockRepo.Setup(x => x.GetCartByCustomerIdAsync(1)).ReturnsAsync(sampleCartItems);
            var controller = new CartController(mockRepo.Object);
            SetUserContext(controller, "1");

            var result = await controller.GetCart(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var model = Assert.IsAssignableFrom<IEnumerable<CartItem>>(okResult.Value);
            Assert.Equal(2, model.Count());
        }
    }
}
