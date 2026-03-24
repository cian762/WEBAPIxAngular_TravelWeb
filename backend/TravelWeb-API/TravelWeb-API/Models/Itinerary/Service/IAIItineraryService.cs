using TravelWeb_API.Models.Itinerary.DTO;

namespace TravelWeb_API.Models.Itinerary.Service
{
    public interface IAIItineraryService
    {
        public Task<int> GenerateNewItineraryAsync(ItineraryCreateDto dto, List<int> selectedPoiIds, int days, string memberid);

    }
}
