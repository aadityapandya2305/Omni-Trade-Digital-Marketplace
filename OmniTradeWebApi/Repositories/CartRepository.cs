using Microsoft.EntityFrameworkCore;
using OmniTradeWebApi.Data;
using OmniTradeWebApi.Models;

namespace OmniTradeWebApi.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly OmniTradeHubContext _context;

        public CartRepository(OmniTradeHubContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CartItem>> GetCartByCustomerIdAsync(int customerId)
        {
            return await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task AddToCartAsync(CartItem cartItem)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == cartItem.ProductId);

            if (product == null)
            {
                throw new InvalidOperationException("Product not found.");
            }

            if (cartItem.Quantity <= 0)
            {
                throw new InvalidOperationException(
                    "Quantity must be greater than zero.");
            }

            var existingCartItem = await _context.CartItems
                .FirstOrDefaultAsync(c =>
                    c.CustomerId == cartItem.CustomerId &&
                    c.ProductId == cartItem.ProductId);

            var newQuantity = cartItem.Quantity;

            if (existingCartItem != null)
            {
                newQuantity += existingCartItem.Quantity;
            }

            if (newQuantity > product.StockQuantity)
            {
                throw new InvalidOperationException(
                    "Requested quantity exceeds available stock.");
            }

            if (existingCartItem != null)
            {
                existingCartItem.Quantity = newQuantity;
            }
            else
            {
                await _context.CartItems.AddAsync(cartItem);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateCartQuantityAsync(
            int cartItemId,
            int quantity)
        {
            if (quantity <= 0)
            {
                throw new InvalidOperationException(
                    "Quantity must be greater than zero.");
            }

            var cartItem = await _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == cartItemId);

            if (cartItem == null)
            {
                throw new InvalidOperationException(
                    "Cart item not found.");
            }

            if (quantity > cartItem.Product.StockQuantity)
            {
                throw new InvalidOperationException(
                    "Requested quantity exceeds available stock.");
            }

            cartItem.Quantity = quantity;

            await _context.SaveChangesAsync();
        }

        public async Task RemoveFromCartAsync(int cartItemId)
        {
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == cartItemId);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);

                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(int customerId)
        {
            var cartItems = await _context.CartItems
                .Where(c => c.CustomerId == customerId)
                .ToListAsync();

            if (cartItems.Count > 0)
            {
                _context.CartItems.RemoveRange(cartItems);

                await _context.SaveChangesAsync();
            }
        }

        public async Task<CartItem?> GetCartItemByIdAsync(int cartItemId)
        {
            return await _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == cartItemId);
        }
    }
}