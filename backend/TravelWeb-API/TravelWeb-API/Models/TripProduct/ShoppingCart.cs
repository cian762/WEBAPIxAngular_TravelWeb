using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelWeb_API.Models.TripProduct;

public partial class ShoppingCart
{
    public int CartId { get; set; }

    public int? TicketCategoryId { get; set; }

    public string MemberId { get; set; } = null!;

    public string? ProductCode { get; set; }

    public int? Quantity { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual MemberInformation? Member { get; set; } 

    public virtual TicketCategory? TicketCategory { get; set; }
}
