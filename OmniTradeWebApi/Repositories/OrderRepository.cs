using Microsoft.EntityFrameworkCore;
using OmniTradeWebApi.Data;
using OmniTradeWebApi.DTOs;
using OmniTradeWebApi.Models;

namespace OmniTradeWebApi.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OmniTradeHubContext _context;

        public OrderRepository(OmniTradeHubContext context)
        {
            _context = context;
        }

        public async Task<Order> CreateOrderFromCartAsync(int customerId)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var cartItems = await _context.CartItems
                    .Include(c => c.Product)
                    .Where(c => c.CustomerId == customerId)
                    .ToListAsync();

                if (cartItems.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Cart is empty.");
                }

                foreach (var cartItem in cartItems)
                {
                    if (cartItem.Quantity <= 0)
                    {
                        throw new InvalidOperationException(
                            "Cart contains an invalid quantity.");
                    }

                    if (cartItem.Product == null)
                    {
                        throw new InvalidOperationException(
                            "A product in the cart could not be found.");
                    }

                    if (cartItem.Quantity >
                        cartItem.Product.StockQuantity)
                    {
                        throw new InvalidOperationException(
                            $"Insufficient stock for product '{cartItem.Product.Name}'.");
                    }
                }

                decimal totalAmount = cartItems.Sum(
                    c => c.Product.Price * c.Quantity);

                var order = new Order
                {
                    CustomerId = customerId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    Status = "Pending"
                };

                await _context.Orders.AddAsync(order);

                foreach (var cartItem in cartItems)
                {
                    var orderItem = new OrderItem
                    {
                        Order = order,
                        ProductId = cartItem.ProductId,
                        VendorId = cartItem.Product.VendorId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.Product.Price
                    };

                    await _context.OrderItems.AddAsync(orderItem);

                    cartItem.Product.StockQuantity -=
                        cartItem.Quantity;
                }

                _context.CartItems.RemoveRange(cartItems);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return order;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderItem>> GetOrderItemsByVendorIdAsync(int vendorId)
        {
            return await _context.OrderItems
                .Include(o => o.Order)
                .Include(o => o.Product)
                .Where(o => o.VendorId == vendorId)
                .ToListAsync();
        }

        public async Task UpdateOrderStatusAsync(int orderId,string status)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new InvalidOperationException(
                    "Order not found.");
            }

            var validStatuses = new[]
            {
                "Pending",
                "Processing",
                "Shipped",
                "Delivered"
            };

            if (!validStatuses.Contains(status))
            {
                throw new InvalidOperationException(
                    "Invalid order status.");
            }

            var validTransition =
                order.Status == "Pending" && status == "Processing"
                || order.Status == "Processing" && status == "Shipped"
                || order.Status == "Shipped" && status == "Delivered";

            if (!validTransition)
            {
                throw new InvalidOperationException(
                    $"Cannot change order status from '{order.Status}' to '{status}'.");
            }

            order.Status = status;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> VendorHasOrderAsync(int orderId, int vendorId)
        {
            return await _context.OrderItems
                .AnyAsync(oi => oi.OrderId == orderId && oi.VendorId == vendorId);
        }

        public async Task<VendorOrderDetailsDto?> GetVendorOrderDetailsAsync(int orderId, int vendorId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o =>
                    o.Id == orderId &&
                    o.OrderItems.Any(oi => oi.VendorId == vendorId));

            if (order == null)
            {
                return null;
            }

            return new VendorOrderDetailsDto
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,

                Items = order.OrderItems
                    .Where(oi => oi.VendorId == vendorId)
                    .Select(oi => new VendorOrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice
                    })
                    .ToList()
            };
        }
    }
}