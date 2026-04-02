namespace TravelWeb_API.Models.MemberSystem;

public partial class MemberInformation
{
    public string MemberId { get; set; } = null!;

    public string MemberCode { get; set; } = null!;

    public string? Name { get; set; }

    public byte? Gender { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Status { get; set; }

    public string? BackgroundUrl { get; set; }

    public virtual ICollection<MemberComplaint> MemberComplaints { get; set; } = new List<MemberComplaint>();

    public virtual ICollection<MemberInformation> Followeds { get; set; } = new List<MemberInformation>();

    public virtual ICollection<MemberInformation> Followers { get; set; } = new List<MemberInformation>();
    public virtual ICollection<MemberInformation> blockedIngs { get; set; } = new List<MemberInformation>();
    public virtual ICollection<MemberInformation>  blockedIds { get; set; } = new List<MemberInformation>();
}
