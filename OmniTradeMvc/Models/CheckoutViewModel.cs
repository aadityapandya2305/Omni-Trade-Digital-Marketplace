using System.ComponentModel.DataAnnotations;

namespace OmniTradeMvc.Models
{
    // Bound to the Checkout Index view (cart summary + shipping/payment form)
    public class CheckoutViewModel
    {
        public CartViewModel Cart { get; set; } = new();

        [Required(ErrorMessage = "Shipping address is required.")]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a payment method.")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = string.Empty;
    } 
}