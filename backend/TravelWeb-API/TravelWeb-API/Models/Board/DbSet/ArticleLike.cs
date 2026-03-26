using System;
using System.Collections.Generic;
using TravelWeb_API.Models.MemberSystem;

namespace TravelWeb_API.Models.Board.DbSet;

public partial class ArticleLike
{
    public int ArticleId { get; set; }

    public string UserId { get; set; } = null!;

    public virtual Article Article { get; set; } = null!;
    public virtual MemberInformation MemberInformation { get; set; } = null!;
}
