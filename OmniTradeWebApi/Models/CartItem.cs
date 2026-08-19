using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;


namespace OmniTradeWebApi.Models;

public partial class CartItem
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    [ValidateNever]
    public virtual User Customer { get; set; } = null!;

    [ValidateNever]
    public virtual Product Product { get; set; } = null!;
}
