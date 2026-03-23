using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.attraction;

public partial class AttractionProductImage
{
    public int ImageId { get; set; }

    public int ProductId { get; set; }

    public string ImagePath { get; set; } = null!;

    public string? Caption { get; set; }

    public int SortOrder { get; set; }

    public virtual AttractionProduct Product { get; set; } = null!;
}

