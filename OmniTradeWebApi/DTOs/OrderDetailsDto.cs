namespace OmniTradeWebApi.DTOs
{
    public class OrderDetailsDto
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public DateTime? OrderDate { get; set; }

        public string Status { get; set; } = null!;

        public string ShippingAddress { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public IEnumerable<OrderDetailsItemDto> Items { get; set; }
            = new List<OrderDetailsItemDto>();
    }

    public class OrderDetailsItemDto
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = null!;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
    }
}