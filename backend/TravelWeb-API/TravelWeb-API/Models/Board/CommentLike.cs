using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.Board;

public partial class CommentLike
{
    public int CommentId { get; set; }

    public int UserId { get; set; }

    public virtual Comment Comment { get; set; } = null!;
}
