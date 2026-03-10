using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.TripProduct;

public partial class MemberInformation
{
    public string MemberId { get; set; } = null!;

    public string MemberCode { get; set; } = null!;

    public string? Name { get; set; }

    public byte? Gender { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<ItineraryProductCollection> ItineraryProductCollections { get; set; } = new List<ItineraryProductCollection>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();
}
