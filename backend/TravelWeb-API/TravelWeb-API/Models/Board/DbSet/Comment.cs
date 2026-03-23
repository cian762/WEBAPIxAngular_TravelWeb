using System;
using System.Collections.Generic;
using TravelWeb_API.Models.MemberSystem;

namespace TravelWeb_API.Models.Board.DbSet;

public partial class Comment
{
    public int CommentId { get; set; }

    public int ArticleId { get; set; }

    public string UserId { get; set; } = null!;

    public int? ParentId { get; set; }

    public string? Contents { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Article Article { get; set; } = null!;
    public virtual MemberInformation MemberInformation { get; set; } = null!;

    public virtual ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();

    // 導覽屬性
    public virtual Comment? Parent { get; set; } // 指向母物件
    public virtual ICollection<Comment> InverseParent { get; set; } = new List<Comment>(); // 所有的子物件
}
