using TravelWeb_API.Models.Itinerary.DBContext;

namespace TravelWeb_API.Models.Itinerary.Service
{
    public class Itineraryservice : IItineraryservice
    {
        private readonly TravelContext _context;
        public Itineraryservice(TravelContext context)
        {
            _context = context;
        }


    }
}
