namespace OmniTradeWebApi.DTOs
{
    public class VendorOrderDetailsDto
    {
        public int OrderId { get; set; }

        public DateTime? OrderDate { get; set; }

        public string Status { get; set; } = null!;

        public decimal TotalAmount { get; set; }

        public IEnumerable<VendorOrderItemDto> Items { get; set; }
            = new List<VendorOrderItemDto>();
    }

    public class VendorOrderItemDto
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = null!;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
    }
}