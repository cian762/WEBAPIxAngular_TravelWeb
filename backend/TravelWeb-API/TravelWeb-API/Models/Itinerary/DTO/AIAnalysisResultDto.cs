using System.Text.Json.Serialization;

namespace TravelWeb_API.Models.Itinerary.DTO
{
    public class AiItineraryResult
    {
        public TripSummary Summary { get; set; }
        public List<DayPlan> Itinerary { get; set; }
        public List<UnplacedPoi> UnplacedPois { get; set; }
    }
    public class UnplacedPoi
    {
        public int? AttractionId { get; set; } // 哪個景點沒排進去
        public string Reason { get; set; }     // AI 給的原因（如：休館、太遠）
    }
    public class DayPlan
    {
        public int Day { get; set; }
        public string Date { get; set; }
        public string AreaFocus { get; set; }
        public DayMetrics DailyMetrics { get; set; }
        public List<ScheduleItem> Schedule { get; set; } // 這是一天內的所有活動
    }

    public class ScheduleItem
    {
        public string Start { get; set; } // "09:00"
        public string End { get; set; }   // "10:30"
        public string Type { get; set; }  // "sightseeing", "meal" 等
        public string Title { get; set; }
        public int? PoiId { get; set; }   // 對應 DB 的 attraction_id (可為 null)
        public string Details { get; set; }
        public decimal ExpectedFatigueGain { get; set; }
    }

    public class DayMetrics
    {
        public decimal Feasibility { get; set; }
        public decimal Fatigue { get; set; }
    }
    public class TripSummary
    {
        public string VersionStrategy { get; set; } // AI 產出的規劃邏輯
        public Metrics AnalysisMetrics { get; set; }
    }

    public class Metrics
    {
        public decimal OverallFeasibility { get; set; }
        public decimal OverallFatigue { get; set; }
        public string Reasoning { get; set; }
    }
    public class AiInputContext
    {
        [JsonPropertyName("destination")]
        public string Destination { get; set; }
        [JsonPropertyName("total_days")]
        public int TotalDays { get; set; }
        [JsonPropertyName("start_date")]
        public DateTime? StartDate { get; set; }
        [JsonPropertyName("attraction_pool")]
        // 景點池：這是 AI 最需要的「約束條件」
        public List<PoiInfoForAi> AttractionPool { get; set; }
    }

    public class PoiInfoForAi
    {
        [JsonPropertyName("attraction_id")]
        public int AttractionId { get; set; } // 重要：對應您的 DB 主鍵
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("latitude")]
        public decimal Latitude { get; set; }
        [JsonPropertyName("longitude")]
        public decimal Longitude { get; set; }
        [JsonPropertyName("business_hours")]
        public string BusinessHours { get; set; }
        [JsonPropertyName("transportinfo")]
        public string TransportInfo { get; set; }
        [JsonPropertyName("must_visit")]
        public bool MustVisit { get; set; } // 用於滿足您的 Core Logic 4
    }
}
