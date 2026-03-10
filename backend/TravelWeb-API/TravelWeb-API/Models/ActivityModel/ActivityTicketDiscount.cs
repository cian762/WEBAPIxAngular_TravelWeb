using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.ActivityModel;

public partial class ActivityTicketDiscount
{
    public int DiscountId { get; set; }

    public double? DiscountRate { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int? Quantity { get; set; }

    public bool? IsEnabled { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AcitivityTicket> ProductCodes { get; set; } = new List<AcitivityTicket>();
}
