using TravelWeb_API.Models.Itinerary.DBModel;

namespace TravelWeb_API.Models.Itinerary.Service
{
    public interface IAnalysisRepository
    {
        Task<Aianalysis?> GetAnalysisByVersionIdAsync(int versionId);
        Task<Aianalysis> CreateAnalysisAsync(Aianalysis entity);
        Task<List<ItineraryVersion>> GetVersionSummariesAsync(int itineraryId);
        Task<ItineraryVersion> GetVersionWithItemsAndAnalysisAsync(int itineraryId, int versionId);
        Task<ItineraryComparison> CreateComparisonAsync(int originalVersionId, int comparedVersionId);
    }
}

