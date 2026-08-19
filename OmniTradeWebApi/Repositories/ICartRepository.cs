using OmniTradeWebApi.Models;

namespace OmniTradeWebApi.Repositories
{
    public interface ICartRepository
    {
        Task<IEnumerable<CartItem>> GetCartByCustomerIdAsync(int customerId);

        Task AddToCartAsync(CartItem cartItem);

        Task UpdateCartQuantityAsync(int cartItemId, int quantity);

        Task RemoveFromCartAsync(int cartItemId);

        Task ClearCartAsync(int customerId);

        Task<CartItem?> GetCartItemByIdAsync(int cartItemId);
    }
}