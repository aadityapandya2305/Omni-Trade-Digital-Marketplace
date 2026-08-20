namespace OmniTradeMvc.Models
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        public int VendorId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public string Category { get; set; } = string.Empty;
    }
}