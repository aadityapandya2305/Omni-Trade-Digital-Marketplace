namespace OmniTradeMvc.Models
{
    public class VendorOrderDetailsViewModel
    {
        public int OrderId { get; set; }

        public DateTime? OrderDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public IEnumerable<VendorOrderItemDetailsViewModel> Items { get; set; }
            = new List<VendorOrderItemDetailsViewModel>();
    }

    public class VendorOrderItemDetailsViewModel
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
    }
}