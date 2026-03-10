using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.TripProduct;

public partial class OrderItemTicket
{
    public int OrderItemTicketId { get; set; }

    public int OrderItemId { get; set; }

    public int TicketCategoryId { get; set; }

    public string? TicketNameSnapshot { get; set; }

    public decimal? UnitPrice { get; set; }

    public int? Quantity { get; set; }

    public virtual OrderItem OrderItem { get; set; } = null!;

    public virtual TicketCategory TicketCategory { get; set; } = null!;
}
