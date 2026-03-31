using System;
using System.Collections.Generic;
using TravelWeb_API.Models.Board.DbSet;

namespace TravelWeb_API.Models.ActivityModel;

public partial class TagsRegion
{
    public int RegionId { get; set; }

    public int? Uid { get; set; }

    public string RegionName { get; set; } = null!;

    public virtual ICollection<TagsRegion> InverseUidNavigation { get; set; } = new List<TagsRegion>();

    public virtual TagsRegion? UidNavigation { get; set; }

    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();

    //public ICollection<Article> Articles { get; set; }= new List<Article>();
}
