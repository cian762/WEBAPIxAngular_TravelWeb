using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models.TripProduct.ITripProduct;
using TravelWeb_API.Models.TripProduct.TripDTO;

namespace TravelWeb_API.Models.TripProduct.STripProduct
{
    public class SOrder:IOrder
    {
        private readonly TripDbContext _context;

        public SOrder(TripDbContext context) 
        {
         _context = context;
        }

        public Task<bool> CancelOrderAsync(int orderId, string memberId)
        {
            throw new NotImplementedException();
        }

        public Task<int> CreateOrderAsync(CreateOrderDto dto, string memberId)
        {
            throw new NotImplementedException();
        }

        public Task<OrderDetailDto> GetCheckoutPreviewAsync(CreateOrderDto dto, string memberId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<OrderListDto>> GetMemberOrdersAsync(string memberId)
        {
            throw new NotImplementedException();
        }

        public Task<OrderDetailDto?> GetOrderDetailAsync(int orderId, string memberId)
        {
            throw new NotImplementedException();
        }
    }
}
