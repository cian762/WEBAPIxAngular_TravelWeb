using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.TripProduct;

public partial class ProductInventoryStatus
{
    public int ProductId { get; set; }

    public string? InventoryMode { get; set; }

    public int? DailyLimit { get; set; }

    public int? SoldQuantity { get; set; }

    public DateTime? LastUpdatedAt { get; set; }

    public virtual AttractionProduct Product { get; set; } = null!;
}
