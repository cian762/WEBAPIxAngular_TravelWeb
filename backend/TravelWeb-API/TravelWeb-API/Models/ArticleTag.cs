using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models;

public partial class ArticleTag
{
    public int ArticleId { get; set; }

    public int TagId { get; set; }

    public virtual Article Article { get; set; } = null!;

    public virtual TagsList Tag { get; set; } = null!;
}
