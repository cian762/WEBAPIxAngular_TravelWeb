using TravelWeb_API.Models.TripProduct.TripDTO;

namespace TravelWeb_API.Models.TripProduct.ITripProduct
{
    public interface IOrder
    {
       Task <IEnumerable<CreateOrderDto>> CheckOrder(CreateOrderDto dto,string member);

    }
}
