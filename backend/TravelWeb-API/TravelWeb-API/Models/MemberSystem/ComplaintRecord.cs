using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelWeb_API.Models.MemberSystem;

public partial class ComplaintRecord
{
    public string AdminId { get; set; } = null!;

    public string ComplaintId { get; set; } = null!;

    public string MemberCode { get; set; } = null!;

    public string? Status { get; set; }

    public string? Compensation { get; set; }

    public virtual Administrator Admin { get; set; } = null!;

    public virtual MemberList MemberCodeNavigation { get; set; } = null!;
   
    public virtual MemberComplaint? MemberComplaint { get; set; }
}
