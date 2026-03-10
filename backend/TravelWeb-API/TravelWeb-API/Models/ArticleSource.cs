using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models;

public partial class ArticleSource
{
    public int ArticleId { get; set; }

    public int Source { get; set; }

    public virtual Article Article { get; set; } = null!;
}
