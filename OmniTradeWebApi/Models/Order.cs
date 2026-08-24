using System;
using System.Collections.Generic;

namespace OmniTradeWebApi.Models;

public partial class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = null!;

    public string? ShippingAddress { get; set; }

    public string? PaymentMethod { get; set; }

    public virtual User Customer { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; }
        = new List<OrderItem>();
}