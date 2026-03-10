using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.attraction;

public partial class AttractionTypeMapping
{
    public int AttractionTypeId { get; set; }

    public int AttractionId { get; set; }

    public virtual Attraction Attraction { get; set; } = null!;

    public virtual AttractionTypeCategory AttractionType { get; set; } = null!;
}
