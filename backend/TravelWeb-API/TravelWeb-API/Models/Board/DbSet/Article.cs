using System;
using System.Collections.Generic;
using TravelWeb_API.Models.MemberSystem;

namespace TravelWeb_API.Models.Board.DbSet;

public partial class Article
{
    public int ArticleId { get; set; }

    public string UserId { get; set; } = null!;

    public string? Title { get; set; }

    public byte Type { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte Status { get; set; }

    public bool IsViolation { get; set; }

    public string? PhotoUrl { get; set; }

    public virtual ICollection<ArticleFolder> ArticleFolders { get; set; } = new List<ArticleFolder>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual Journal? Journal { get; set; }

    public virtual ICollection<JournalPage> JournalPages { get; set; } = new List<JournalPage>();

    public virtual Post? Post { get; set; }

    // 導覽屬性
    public virtual MemberInformation MemberInformation { get; set; } = null!;


}
