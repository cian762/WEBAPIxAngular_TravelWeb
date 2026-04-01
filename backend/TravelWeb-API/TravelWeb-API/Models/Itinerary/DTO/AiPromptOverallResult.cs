using System.Text.Json.Serialization;

namespace TravelWeb_API.Models.Itinerary.DTO
{
    // Dtos/ItineraryAnalysisDto.cs

    // ── AI Prompt 原始輸出（反序列化用）──
    public class AiPromptOverallResult
    {
        [JsonPropertyName("feasibility")] public double Feasibility { get; set; }
        [JsonPropertyName("fatigue")] public double Fatigue { get; set; }
        [JsonPropertyName("reason")] public string Reason { get; set; } = string.Empty;
    }

    public class AiPromptDailyResult
    {
        [JsonPropertyName("day")] public int Day { get; set; }
        [JsonPropertyName("feasibility")] public double Feasibility { get; set; }
        [JsonPropertyName("fatigue")] public double Fatigue { get; set; }
        [JsonPropertyName("route_comment")] public string RouteComment { get; set; } = string.Empty;
    }

    public class AiPromptOutput
    {
        [JsonPropertyName("overall_analysis")] public AiPromptOverallResult OverallAnalysis { get; set; } = new();
        [JsonPropertyName("daily_analysis")] public List<AiPromptDailyResult> DailyAnalysis { get; set; } = new();
    }

    // ── 前端顯示用（圓形進度環）──
    public class AnalysisMetricsDto
    {
        public int AnalysisId { get; set; }
        public int VersionId { get; set; }
        public double FeasibilityScore { get; set; }
        public double PaceBalanceScore { get; set; }
        public double FatigueIndex { get; set; }
        public string Reason { get; set; } = string.Empty;  // overall reason
        public DateTime AnalysisTime { get; set; }
    }

    // ── 版本清單（Dialog 選擇用）──
    public class VersionSummaryDto
    {
        public int VersionId { get; set; }
        public int VersionNumber { get; set; }
        public string? Source { get; set; }
        public string? VersionRemark { get; set; }
        public DateTime CreateTime { get; set; }
        public string CurrentUsageStatus { get; set; } = string.Empty;
        public bool HasAnalysis { get; set; }
    }

    // ── 版本詳細（含 Items + Analysis）──
    public class VersionDetailDto
    {
        public int VersionId { get; set; }
        public int VersionNumber { get; set; }
        public DateTime CreateTime { get; set; }
        public string? VersionRemark { get; set; }
        public List<ItemSnapDto> Items { get; set; } = new();
        public AnalysisMetricsDto? Analysis { get; set; }
    }

    public class ItemSnapDto
    {
        public int DayNumber { get; set; }
        public int SortOrder { get; set; }
        public string AttractionName { get; set; } = string.Empty;
        public string? StartTime { get; set; }  // "HH:mm"
        public string? EndTime { get; set; }
        public string? GooglePlaceId { get; set; }
    }

    // ── 版本比較回傳 ──
    public class VersionCompareResponseDto
    {
        public int ComparisonId { get; set; }
        public VersionDetailDto VersionA { get; set; } = new();
        public VersionDetailDto VersionB { get; set; } = new();
        public List<DiffItemDto> Diffs { get; set; } = new();
    }

    public class DiffItemDto
    {
        public string Label { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string DiffType { get; set; } = string.Empty; // added | removed | changed
    }
}

