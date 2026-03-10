using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.attraction;

public partial class TicketType
{
    public int TicketTypeCode { get; set; }

    public string TicketTypeName { get; set; } = null!;

    public int? SortOrder { get; set; }

    public virtual ICollection<AttractionProduct> AttractionProducts { get; set; } = new List<AttractionProduct>();
}
