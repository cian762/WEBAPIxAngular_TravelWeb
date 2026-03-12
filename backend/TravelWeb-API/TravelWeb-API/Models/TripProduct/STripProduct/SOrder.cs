using TravelWeb_API.Models.TripProduct.ITripProduct;

namespace TravelWeb_API.Models.TripProduct.STripProduct
{
    public class SOrder:IOrder
    {
        private readonly TripDbContext _context;

        public SOrder(TripDbContext context) 
        {
         _context = context;
        }


    }
}
