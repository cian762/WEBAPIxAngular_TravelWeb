using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.Board.DbSet;

public partial class TagsList
{
    public int TagId { get; set; }

    public string TagName { get; set; } = null!;
}
