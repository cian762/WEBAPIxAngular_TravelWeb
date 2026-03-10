using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.Board;

public partial class Post
{
    public int ArticleId { get; set; }

    public string? Contents { get; set; }

    public int? RegionId { get; set; }

    public virtual Article Article { get; set; } = null!;
}
