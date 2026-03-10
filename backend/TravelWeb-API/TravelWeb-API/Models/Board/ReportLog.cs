using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.Board;

public partial class ReportLog
{
    public int LogId { get; set; }

    public int TargetId { get; set; }

    public byte TargetType { get; set; }

    public string? UserId { get; set; }

    public byte ViolationType { get; set; }

    public string? Reason { get; set; }

    public string? Photo { get; set; }

    public string? Snapshot { get; set; }

    public DateTime CreatedAt { get; set; }

    public byte ResultType { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte Status { get; set; }
}
