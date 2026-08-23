using OmniTradeMvc.Models;

namespace OmniTradeMvc.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<VendorOrderItemViewModel>> GetIncomingOrdersAsync(
            int vendorId);

        Task<VendorOrderDetailsViewModel?> GetOrderDetailsAsync(
            int vendorId,
            int orderId);

        Task<bool> UpdateOrderStatusAsync(int orderId, string status);

        Task<int?> GetCurrentVendorIdAsync();
    }
}