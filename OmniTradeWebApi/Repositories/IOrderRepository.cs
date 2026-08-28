using OmniTradeWebApi.DTOs;
using OmniTradeWebApi.Models;

namespace OmniTradeWebApi.Repositories
{
    public interface IOrderRepository
    {
        Task<OrderDetailsDto> CreateOrderFromCartAsync(int customerId);

        Task<IEnumerable<OrderDetailsDto>> GetOrdersByCustomerIdAsync(int customerId);

        Task<OrderDetailsDto?> GetOrderDetailsForCustomerAsync(int orderId, int customerId);

        Task<IEnumerable<OrderItem>> GetOrderItemsByVendorIdAsync(int vendorId);

        Task UpdateOrderStatusAsync(int orderId, string status);

        Task<bool> VendorHasOrderAsync(int orderId, int vendorId);

        Task<VendorOrderDetailsDto?> GetVendorOrderDetailsAsync(int orderId, int vendorId);
    }
}