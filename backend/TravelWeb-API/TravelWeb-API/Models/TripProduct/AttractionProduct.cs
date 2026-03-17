using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.TripProduct;

public partial class AttractionProduct
{
    public int ProductId { get; set; }

    public string ProductCode { get; set; } = null!;

    public int AttractionId { get; set; }

    public string Title { get; set; } = null!;

    public string? Status { get; set; }

    public int? PolicyId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public decimal? Price { get; set; }

    public int? MaxPurchaseQuantity { get; set; }

    public int? IsActive { get; set; }

    public int? TicketTypeCode { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Attraction Attraction { get; set; } = null!;

    public virtual ICollection<StockInRecord> StockInRecords { get; set; } = new List<StockInRecord>();
}
