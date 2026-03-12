using System;
using System.Collections.Generic;

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

    public virtual ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();
}
