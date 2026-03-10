using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.TripProduct;

public partial class ResourcesImage
{
    public int MainImageid { get; set; }

    public string? MainImage { get; set; }

    public int? ResourceId { get; set; }

    public virtual Resource? Resource { get; set; }
}
