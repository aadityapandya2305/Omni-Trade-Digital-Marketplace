namespace OmniTradeWebApi.DTOs
{
    public class VendorManagementDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string StoreName { get; set; }

        public string ContactEmail { get; set; }

        public bool? IsApproved { get; set; }
    }
}