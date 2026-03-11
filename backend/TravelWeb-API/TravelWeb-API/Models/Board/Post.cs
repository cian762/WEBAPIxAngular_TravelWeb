using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TravelWeb_API.Models.Board;

public partial class Post
{
    [Key]
    public int ArticleId { get; set; }

    public string? Contents { get; set; }

    public int? RegionId { get; set; }

    public virtual Article Article { get; set; } = null!;
}
