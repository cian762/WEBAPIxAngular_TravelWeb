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

        public async Task<IEnumerable<CreateOrderDto>> CheckOrder(CreateOrderDto dto, string member)
        {
           var order = await _context.Orders.Where(s=>s.MemberId==member).Select(o=>new CreateOrderDto { 
             MemberId = o.MemberId,
             ContactEmail = o.ContactEmail!,
             ContactName = o.ContactName!,
             ContactPhone = o.ContactPhone!,
             CustomerNote = o.CustomerNote!,
           }).ToListAsync();
            return  order;
        }
    }
}
