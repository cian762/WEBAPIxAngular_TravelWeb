using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.TripProduct;

public partial class QrcodeVerification
{
    public int VerificationId { get; set; }

    public int QrcodeId { get; set; }

    public string Action { get; set; } = null!;

    public string Message { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string OperatorId { get; set; } = null!;

    public virtual QrcodeInfo Qrcode { get; set; } = null!;
}
