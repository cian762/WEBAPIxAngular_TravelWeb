using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.Board.DbSet;

public partial class UserActivityLog
{
    public int LogId { get; set; }

    public string UserId { get; set; } = null!;

    public byte? TargetType { get; set; }

    public int TargetId { get; set; }

    public byte ActionType { get; set; }

    public DateTime CreatedAt { get; set; }
}
