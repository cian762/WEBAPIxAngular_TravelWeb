using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models;

public partial class UserActivityLog
{
    public int LogId { get; set; }

    public int UserId { get; set; }

    public byte TargetType { get; set; }

    public int TargetId { get; set; }

    public byte ActionType { get; set; }

    public DateTime CreatedAt { get; set; }
}
