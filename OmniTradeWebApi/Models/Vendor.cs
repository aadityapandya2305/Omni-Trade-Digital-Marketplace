using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OmniTradeWebApi.Models;

public partial class Vendor
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string StoreName { get; set; } = null!;

    public string? Description { get; set; }

    public string ContactEmail { get; set; } = null!;

    public bool? IsApproved { get; set; }

    [JsonIgnore]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [JsonIgnore]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    [ValidateNever]
    public virtual User User { get; set; } = null!;
}
