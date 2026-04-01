using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.TripProduct;

public partial class QrcodeInfo
{
    public int QrcodeId { get; set; }

    public int OrderId { get; set; }

    public int OrderItemId { get; set; }

    public string Qrtoken { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? UsedAt { get; set; }

    public string? UsedBy { get; set; }

    public DateTime CreateAt { get; set; }

    public DateOnly? ExpiredDate { get; set; }

    public virtual OrderItem Order { get; set; } = null!;

    public virtual ICollection<QrcodeVerification> QrcodeVerifications { get; set; } = new List<QrcodeVerification>();
}
