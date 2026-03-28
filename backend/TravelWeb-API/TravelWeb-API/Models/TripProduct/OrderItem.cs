using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.TripProduct;

public partial class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }

    public string? ProductCode { get; set; }

    public string? ProductNameSnapshot { get; set; }

    public DateOnly? StartDateSnapshot { get; set; }

    public DateOnly? EndDateSnapshot { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ICollection<OrderItemTicket> OrderItemTickets { get; set; } = new List<OrderItemTicket>();

    public virtual ICollection<QrcodeInfo> QrcodeInfos { get; set; } = new List<QrcodeInfo>();
}
