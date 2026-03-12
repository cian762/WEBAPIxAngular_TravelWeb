using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.Board.DbSet;

public partial class CommentPhoto
{
    public int CommentId { get; set; }

    public string? Photo { get; set; }

    public virtual Comment Comment { get; set; } = null!;
}
