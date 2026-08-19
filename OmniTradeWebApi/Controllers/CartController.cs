using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniTradeWebApi.Models;
using OmniTradeWebApi.Repositories;
using System.Security.Claims;

namespace OmniTradeWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _cartRepository;

        public CartController(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        [HttpGet("{customerId}")]
        public async Task<ActionResult<IEnumerable<CartItem>>> GetCart(int customerId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            if (userId != customerId)
            {
                return Forbid();
            }

            var cart = await _cartRepository
                .GetCartByCustomerIdAsync(customerId);

            return Ok(cart);
        }

        [HttpPost]
        public async Task<ActionResult> AddToCart(CartItem cartItem)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            if (userId != cartItem.CustomerId)
            {
                return Forbid();
            }

            try
            {
                await _cartRepository.AddToCartAsync(cartItem);

                return Ok(new
                {
                    message = "Item added to cart successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCartQuantity(
    int id,
    [FromBody] int quantity)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var cartItem = await _cartRepository
                .GetCartItemByIdAsync(id);

            if (cartItem == null)
            {
                return NotFound("Cart item not found.");
            }

            if (cartItem.CustomerId != userId)
            {
                return Forbid();
            }

            try
            {
                await _cartRepository
                    .UpdateCartQuantityAsync(id, quantity);

                return Ok(new
                {
                    message = "Cart quantity updated successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> RemoveFromCart(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var cartItem = await _cartRepository
                .GetCartItemByIdAsync(id);

            if (cartItem == null)
            {
                return NotFound("Cart item not found.");
            }

            if (cartItem.CustomerId != userId)
            {
                return Forbid();
            }

            await _cartRepository.RemoveFromCartAsync(id);

            return Ok(new
            {
                message = "Item removed from cart successfully."
            });
        }
    }
}