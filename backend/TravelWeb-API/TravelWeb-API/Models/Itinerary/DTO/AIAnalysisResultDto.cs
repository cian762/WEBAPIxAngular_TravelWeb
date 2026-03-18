namespace TravelWeb_API.Models.Itinerary.DTO
{
    public class AIAnalysisResultDto
    {
        public MetricsDto AnalysisMetrics { get; set; }
        public string Reasoning { get; set; }
    }

    public class MetricsDto
    {
        public double OverallFeasibility { get; set; }
        public double OverallFatigue { get; set; }
    }
    public class analysisDto
    {

    }
}
