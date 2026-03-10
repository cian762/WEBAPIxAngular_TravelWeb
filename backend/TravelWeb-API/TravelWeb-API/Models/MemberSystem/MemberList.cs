using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.MemberSystem;

public partial class MemberList
{
    public string MemberCode { get; set; } = null!;

    public string? Email { get; set; }

    public string? PasswordHash { get; set; }

    public string? Phone { get; set; }

    public virtual ICollection<Authorization> Authorizations { get; set; } = new List<Authorization>();

    public virtual ICollection<ComplaintRecord> ComplaintRecords { get; set; } = new List<ComplaintRecord>();

    public virtual ICollection<LogInRecord> LogInRecords { get; set; } = new List<LogInRecord>();
}
