using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.Board;

public partial class AuditNote
{
    public int TargetId { get; set; }

    public string? Note { get; set; }
}
