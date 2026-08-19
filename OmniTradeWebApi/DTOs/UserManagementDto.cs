namespace OmniTradeWebApi.DTOs
{
    public class UserManagementDto
    {
        public int Id { get; set; }

        public string Username { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Role { get; set; } = null!;

        public DateTime? CreatedAt { get; set; }
    }
}