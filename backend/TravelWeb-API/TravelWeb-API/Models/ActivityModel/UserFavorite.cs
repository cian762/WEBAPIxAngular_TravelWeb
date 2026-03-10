using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.ActivityModel;

public partial class UserFavorite
{
    public int FavoriteId { get; set; }

    public string? UserId { get; set; }

    public int? ActivityId { get; set; }

    public virtual Activity? Activity { get; set; }
}
