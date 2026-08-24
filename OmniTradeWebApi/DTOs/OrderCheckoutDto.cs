namespace OmniTradeWebApi.DTOs
{
    public class OrderCheckoutDto
    {
        public string ShippingAddress { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = string.Empty;
    }
}