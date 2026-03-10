using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.Board;

public partial class UserSearchHistory
{
    public int SearchId { get; set; }

    public int UserId { get; set; }

    public string? Keywords { get; set; }

    public int? ResultCount { get; set; }

    public DateTime CreatedAt { get; set; }
}
