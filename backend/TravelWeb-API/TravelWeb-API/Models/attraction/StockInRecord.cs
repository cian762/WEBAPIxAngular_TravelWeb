using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.attraction;

public partial class StockInRecord
{
    public int StockInId { get; set; }

    public string ProductType { get; set; } = null!;

    public string ProductCode { get; set; } = null!;

    public string? SupplierName { get; set; }

    public decimal UnitCost { get; set; }

    public int Quantity { get; set; }

    public int RemainingStock { get; set; }

    public string? InventoryType { get; set; }

    public DateOnly? StockInDate { get; set; }

    public string? Remarks { get; set; }

    public virtual AttractionProduct ProductCodeNavigation { get; set; } = null!;
}
