namespace OmniTradeMvc.Models
{
    public class VendorManagementViewModel
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string StoreName { get; set; } = null!;

        public string ContactEmail { get; set; } = null!;

        public bool? IsApproved { get; set; }
    }
}