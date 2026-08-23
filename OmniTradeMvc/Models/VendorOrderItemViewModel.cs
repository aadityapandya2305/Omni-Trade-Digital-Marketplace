namespace OmniTradeMvc.Models
{
    public class VendorOrderItemViewModel
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public int VendorId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public ProductViewModel? Product { get; set; }
    }
}