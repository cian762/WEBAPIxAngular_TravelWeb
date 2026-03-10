using TravelWeb_API.Models.TripProduct.TripDTO;

namespace TravelWeb_API.Models.TripProduct.ITripProduct
{
    public interface ITripproductTable
    {
        Task<IEnumerable<TripProductDTO>> GetAllAsync();

    }
}
