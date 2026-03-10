using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.TripProduct;

public partial class TripSchedule
{
    public string ProductCode { get; set; } = null!;

    public int TripProductId { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int? MaxCapacity { get; set; }

    public int? SoldQuantity { get; set; }

    public decimal? Price { get; set; }

    public string? Status { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual TripProduct TripProduct { get; set; } = null!;

    public virtual ICollection<TicketCategory> TicketCategories { get; set; } = new List<TicketCategory>();
}
