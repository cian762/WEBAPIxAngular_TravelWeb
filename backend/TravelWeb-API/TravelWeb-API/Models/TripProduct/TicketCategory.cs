using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.TripProduct;

public partial class TicketCategory
{
    public int TicketCategoryId { get; set; }

    public string? CategoryName { get; set; }

    public virtual ICollection<AcitivityTicket> AcitivityTickets { get; set; } = new List<AcitivityTicket>();

    public virtual ICollection<OrderItemTicket> OrderItemTickets { get; set; } = new List<OrderItemTicket>();

    public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();

    public virtual ICollection<TripSchedule> ProductCodes { get; set; } = new List<TripSchedule>();
}
