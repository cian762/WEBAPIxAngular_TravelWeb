using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.MemberSystem;

public partial class Blocked
{
    public string MemberId { get; set; } = null!;

    public string? BlockedId { get; set; }

    public DateOnly? BlockedDate { get; set; }

    public string? Reason { get; set; }
}
