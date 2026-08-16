using System.ComponentModel.DataAnnotations;

namespace OmniTradeWebApi.DTOs
{
    public class RegisterDto
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = null!;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = null!;

        [Required]
        [RegularExpression(
            "^(Customer|Vendor)$",
            ErrorMessage = "Account type must be either Customer or Vendor.")]
        public string AccountType { get; set; } = null!;
    }
}