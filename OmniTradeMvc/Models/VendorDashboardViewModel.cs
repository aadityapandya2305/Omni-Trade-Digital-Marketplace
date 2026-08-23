namespace OmniTradeMvc.Models
{
    public class VendorDashboardViewModel
    {
        public int VendorId { get; set; }

        public string StoreName { get; set; } = string.Empty;

        public int TotalProducts { get; set; }

        public int TotalStock { get; set; }

        public bool IsApproved { get; set; }
    }
}