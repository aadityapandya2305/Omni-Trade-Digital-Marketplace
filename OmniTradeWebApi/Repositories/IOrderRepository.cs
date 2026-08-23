using OmniTradeWebApi.DTOs;
using OmniTradeWebApi.Models;

namespace OmniTradeWebApi.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrderFromCartAsync(int customerId);

        Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId);

        Task<IEnumerable<OrderItem>> GetOrderItemsByVendorIdAsync(int vendorId);

        Task UpdateOrderStatusAsync(int orderId, string status);

        Task<bool> VendorHasOrderAsync(int orderId, int vendorId);

        Task<VendorOrderDetailsDto?> GetVendorOrderDetailsAsync(int orderId, int vendorId);
    }
}