using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models;

public partial class Comment
{
    public int CommentId { get; set; }

    public int ArticleId { get; set; }

    public int UserId { get; set; }

    public int? ParentId { get; set; }

    public string? Contents { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Article Article { get; set; } = null!;
}
