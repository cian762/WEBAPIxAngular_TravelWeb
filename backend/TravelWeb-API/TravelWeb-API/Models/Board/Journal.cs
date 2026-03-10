using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.Board;

public partial class Journal
{
    public int ArticleId { get; set; }

    public byte? CoverId { get; set; }

    public DateOnly? ScheduledDate { get; set; }

    public int? TemplateId { get; set; }

    public virtual Article Article { get; set; } = null!;
}
