using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.TripProduct;

public partial class PaymentTransaction
{
    public int TransactionId { get; set; }

    public int OrderId { get; set; }

    public string? PaymentProvider { get; set; }

    public string? ProviderTransactionNo { get; set; }

    public decimal? PaidAmount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? TransactionStatus { get; set; }

    public string? ResponseCode { get; set; }

    public string? RawResponse { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
