using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.attraction;

public partial class AttractionLike
{
    public int LikeId { get; set; }

    public int AttractionId { get; set; }

    public string? IpAddress { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Attraction Attraction { get; set; } = null!;
}
