using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.TripProduct;

public partial class Order
{
    public int OrderId { get; set; }

    public string MemberId { get; set; } = null!;

    public string? ContactName { get; set; }

    public string? ContactPhone { get; set; }

    public string? ContactEmail { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? OrderStatus { get; set; }

    public string? PaymentStatus { get; set; }

    public string? CustomerNote { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual MemberInformation Member { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
}
