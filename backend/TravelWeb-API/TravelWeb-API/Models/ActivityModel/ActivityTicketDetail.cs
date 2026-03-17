using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.ActivityModel;

public partial class ActivityTicketDetail
{
    public string ProductCode { get; set; } = null!;

    public int? ActivityId { get; set; }

    public string? ProdcutDescription { get; set; }

    public string? TermsOfService { get; set; }

    public string? Note { get; set; }

    public virtual Activity? Activity { get; set; }

    public virtual AcitivityTicket ProductCodeNavigation { get; set; } = null!;
}
