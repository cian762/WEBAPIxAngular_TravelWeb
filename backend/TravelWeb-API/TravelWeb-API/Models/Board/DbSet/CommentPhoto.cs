using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.Board.DbSet;

public partial class CommentPhoto
{
    public int ID { get; set; }
    public int CommentId { get; set; }

    public string Photo { get; set; } = null!;

    public virtual Comment Comment { get; set; } = null!;
}
