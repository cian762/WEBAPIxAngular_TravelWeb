using TravelWeb_API.Models.Itinerary.DTO;

namespace TravelWeb_API.Models.Itinerary.Service
{
    public interface IItineraryAnalysisService
    {
        Task<AnalysisMetricsDto> GetOrCreateAnalysisAsync(int itineraryId, int versionId);
        Task<List<VersionSummaryDto>> GetVersionListAsync(int itineraryId);
        Task<VersionCompareResponseDto> CompareVersionsAsync(
            int itineraryId, int versionAId, int versionBId);
    }
}
