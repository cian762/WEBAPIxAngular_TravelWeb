using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.TripProduct;

public partial class CancellationPolicy
{
    public int PolicyId { get; set; }

    public string? PolicyName { get; set; }

    public int? DaysBefore { get; set; }

    public decimal? RefundRate { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<TripProduct> TripProducts { get; set; } = new List<TripProduct>();
}
