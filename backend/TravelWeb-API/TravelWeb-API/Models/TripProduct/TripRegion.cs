using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.TripProduct;

public partial class TripRegion
{
    public int RegionId { get; set; }

    public string RegionName { get; set; } = null!;

    public virtual ICollection<TripProduct> TripProducts { get; set; } = new List<TripProduct>();
}
