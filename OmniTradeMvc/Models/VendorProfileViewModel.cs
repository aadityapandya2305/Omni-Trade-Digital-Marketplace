using System.ComponentModel.DataAnnotations;

namespace OmniTradeMvc.Models
{
    public class VendorProfileViewModel
    {
        public int VendorId { get; set; }

        [Required(ErrorMessage = "Store name is required.")]
        [StringLength(150, ErrorMessage = "Store name cannot exceed 150 characters.")]
        [Display(Name = "Store Name")]
        public string StoreName { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Contact email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Contact Email")]
        public string ContactEmail { get; set; } = string.Empty;

        public bool? IsApproved { get; set; }
    }
}