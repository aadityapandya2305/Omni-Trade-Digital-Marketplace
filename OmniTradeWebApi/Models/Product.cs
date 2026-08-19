using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace OmniTradeWebApi.Models;

public partial class Product
{
    public int Id { get; set; }

    public int VendorId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public string Category { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [ValidateNever]
    public virtual Vendor Vendor { get; set; } = null!;
}
