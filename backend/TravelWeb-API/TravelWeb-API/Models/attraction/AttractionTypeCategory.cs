using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.attraction;

public partial class AttractionTypeCategory
{
    public int AttractionTypeId { get; set; }

    public string AttractionTypeName { get; set; } = null!;
}
