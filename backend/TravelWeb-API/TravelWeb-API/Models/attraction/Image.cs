using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.attraction;

public partial class Image
{
    public int? AttractionId { get; set; }

    public string? ImagePath { get; set; }

    public int ImageId { get; set; }

    public virtual Attraction? Attraction { get; set; }
}
