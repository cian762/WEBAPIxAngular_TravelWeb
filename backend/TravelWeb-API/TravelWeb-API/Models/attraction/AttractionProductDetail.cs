using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.attraction;

public partial class AttractionProductDetail
{
    public int ProductId { get; set; }

    public string? ContentDetails { get; set; }    // 活動介紹主文

    public string? Notes { get; set; }             // 兌換期 / 備註說明

    public string? UsageInstructions { get; set; } // 如何使用 / 注意事項

    public string? Includes { get; set; }          // 費用包含（換行分隔）

    public string? Excludes { get; set; }          // 費用不包含（換行分隔）

    public string? Eligibility { get; set; }       // 預訂資格（換行分隔）

    public string? CancelPolicy { get; set; }      // 取消政策說明

    public DateTime? LastUpdatedAt { get; set; }
    public string? ValidityNote { get; set; }   // ← 補上
    public virtual AttractionProduct Product { get; set; } = null!;
}
