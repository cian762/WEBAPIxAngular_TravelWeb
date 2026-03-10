
using TravelWeb_API.Models.Itinerary.DTO;

namespace TravelWeb_API.Models.Itinerary.Service
{
    public interface IItineraryservice
    {
        public Task<int> CreateItineraryWithItemsAsync(ItineraryCreateDto dto);
    }
}
