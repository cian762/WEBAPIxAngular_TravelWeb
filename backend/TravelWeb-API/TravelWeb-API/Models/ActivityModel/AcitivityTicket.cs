using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.ActivityModel;

public partial class AcitivityTicket
{
    public string ProductCode { get; set; } = null!;

    public string? ProductName { get; set; }

    public int? TicketCategoryId { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public int? CurrentPrice { get; set; }

    public string? ProductLink { get; set; }

    public string? CoverImageUrl { get; set; }

    public string? Status { get; set; }

    public virtual ActivityTicketDetail? ActivityTicketDetail { get; set; }

    public virtual TicketCategory? TicketCategory { get; set; }

    public virtual ICollection<ActivityTicketDiscount> Discounts { get; set; } = new List<ActivityTicketDiscount>();
}
