using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models.Itinerary.DBContext;
using TravelWeb_API.Models.Itinerary.DBModel;

namespace TravelWeb_API.Models.Itinerary.Service
{
    public class AnalysisRepository : IAnalysisRepository
    {
        private readonly TravelContext _db;
        public AnalysisRepository(TravelContext db) => _db = db;

        public Task<Aianalysis?> GetAnalysisByVersionIdAsync(int versionId) =>
            _db.Aianalyses.FirstOrDefaultAsync(a => a.VersionId == versionId);

        public async Task<Aianalysis> CreateAnalysisAsync(Aianalysis entity)
        {
            _db.Aianalyses.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public Task<List<ItineraryVersion>> GetVersionSummariesAsync(int itineraryId) =>
            _db.ItineraryVersions
               .Where(v => v.ItineraryId == itineraryId)
               .OrderByDescending(v => v.VersionNumber)
               .Include(v => v.Aianalyses)   // 修正：原本 .Analysis 不存在
               .ToListAsync();

        public async Task<ItineraryVersion> GetVersionWithItemsAndAnalysisAsync(
            int itineraryId, int versionId)
        {
            return await _db.ItineraryVersions
                            .Where(v => v.ItineraryId == itineraryId
                                     && v.VersionId == versionId)
                            .Include(v => v.ItineraryItems)
                            .Include(v => v.Aianalyses)
                            .FirstOrDefaultAsync()
                   ?? throw new KeyNotFoundException($"Version {versionId} not found");
        }

        public async Task<ItineraryComparison> CreateComparisonAsync(
            int originalVersionId, int comparedVersionId)
        {
            var entity = new ItineraryComparison
            {
                OriginalVersionId = originalVersionId,
                ComparedVersionId = comparedVersionId,
                ComparisonTime = DateTime.UtcNow
            };
            _db.ItineraryComparisons.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
    }
}
