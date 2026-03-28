
using TravelWeb_API.Models.Itinerary.DTO;

namespace TravelWeb_API.Models.Itinerary.Service
{
    public interface IItineraryservice
    {
        public Task<int> CreateItineraryWithItemsAsync(ItineraryCreateDto dto, string memberid);
        public Task<ItineraryDetailDto> GetItineraryDetailAsync(int itineraryId);
        public Task<int> SaveItinerarySnapshotAsync(ItinerarySnapshotDto dto);
        public Task<bool> SoftDeleteItineraryAsync(int itineraryId);
        public Task<List<ItineraryVersionHistoryDto>> GetVersionHistoryAsync(int itineraryId);
        public Task<VersionDto> GetItemByVersionAsync(int versionId);
        public Task<string> SaveImagebyid(IFormFile image, int Id);
        public Task<DateTime> ExtendOneDayAsync(int itineraryId);
        public Task<byte[]> GeneratePdfAsync(ItineraryExportDto data);
        public Task<byte[]> GetExportFileAsync(int itineraryId);
        public Task<DayItineraryDto> GetDayItineraryAsync(int itineraryId, int dayNumber);
    }
}
