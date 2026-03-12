using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.Board.DbSet;

public partial class PostPhoto
{
    public int ArticleId { get; set; }

    public string Photo { get; set; } = null!;

    public virtual Article Article { get; set; } = null!;
}
