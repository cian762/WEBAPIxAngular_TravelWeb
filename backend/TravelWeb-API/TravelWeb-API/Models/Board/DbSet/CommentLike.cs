using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.Board.DbSet;

public partial class CommentLike
{
    public int CommentId { get; set; }

    public string UserId { get; set; }

    public virtual Comment Comment { get; set; } = null!;
}
