using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniTradeWebApi.Repositories;
using System.Security.Claims;

namespace OmniTradeWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IVendorRepository _vendorRepository;

        public OrdersController(
            IOrderRepository orderRepository,
            IVendorRepository vendorRepository)
        {
            _orderRepository = orderRepository;
            _vendorRepository = vendorRepository;
        }

        [HttpPost("checkout/{customerId}")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult> Checkout(int customerId)
        {
            var userIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            if (userId != customerId)
            {
                return Forbid();
            }

            try
            {
                var order =
                    await _orderRepository
                        .CreateOrderFromCartAsync(customerId);

                return Ok(order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("customer/{customerId}")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult> GetCustomerOrders(int customerId)
        {
            var userIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            if (userId != customerId)
            {
                return Forbid();
            }

            var orders =
                await _orderRepository
                    .GetOrdersByCustomerIdAsync(customerId);

            return Ok(orders);
        }

        [HttpGet("vendor/{vendorId}")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult> GetVendorOrders(int vendorId)
        {
            var userIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var vendor =
                await _vendorRepository.GetVendorByUserIdAsync(userId);

            if (vendor == null)
            {
                return Unauthorized();
            }

            if (vendor.Id != vendorId)
            {
                return Forbid();
            }

            var orderItems =
                await _orderRepository
                    .GetOrderItemsByVendorIdAsync(vendorId);

            return Ok(orderItems);
        }

        [HttpPatch("{orderId}/status")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult> UpdateOrderStatus(
    int orderId,
    [FromBody] string status)
        {
            var userIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var vendor =
                await _vendorRepository.GetVendorByUserIdAsync(userId);

            if (vendor == null)
            {
                return Unauthorized();
            }

            var hasOrder =
                await _orderRepository
                    .VendorHasOrderAsync(orderId, vendor.Id);

            if (!hasOrder)
            {
                return Forbid();
            }

            try
            {
                await _orderRepository
                    .UpdateOrderStatusAsync(orderId, status);

                return Ok(new
                {
                    message = "Order status updated successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("vendor/{vendorId}/{orderId}")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult> GetVendorOrderDetails(int vendorId, int orderId)
        {
            var userIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var vendor =
                await _vendorRepository.GetVendorByUserIdAsync(userId);

            if (vendor == null)
            {
                return Unauthorized();
            }

            if (vendor.Id != vendorId)
            {
                return Forbid();
            }

            var orderDetails =
                await _orderRepository.GetVendorOrderDetailsAsync(
                    orderId,
                    vendorId);

            if (orderDetails == null)
            {
                return NotFound("Order not found.");
            }

            return Ok(orderDetails);
        }
    }
}