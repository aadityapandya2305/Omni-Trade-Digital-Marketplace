using System.Collections.Generic;
using System.Linq;

namespace OmniTradeMvc.Models
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();

        public decimal TotalAmount
        {
            get
            {
                return Items.Sum(item => item.Price * item.Quantity);
            }
        }
    }

    public class CartItemViewModel
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public decimal Total
        {
            get
            {
                return Price * Quantity;
            }
        }
    }
}