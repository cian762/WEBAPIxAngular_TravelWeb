using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.attraction;

public partial class AttractionProductDetail
{
    public int ProductId { get; set; }

    public string? ContentDetails { get; set; }

    public string? Notes { get; set; }

    public string? UsageInstructions { get; set; }

    public DateTime? LastUpdatedAt { get; set; }

    public virtual AttractionProduct Product { get; set; } = null!;
}
