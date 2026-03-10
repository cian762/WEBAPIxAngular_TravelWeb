using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.attraction;

public partial class AttractionProductFavorite
{
    public int FavoriteId { get; set; }

    public string UserId { get; set; } = null!;

    public int ProductId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual AttractionProduct Product { get; set; } = null!;
}
