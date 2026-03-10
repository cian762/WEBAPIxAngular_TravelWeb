using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models.TripProduct.ITripProduct;
using TravelWeb_API.Models.TripProduct.TripDTO;

namespace TravelWeb_API.Models.TripProduct.STripProduct
{
    public class TripproductTable : ITripproductTable
    {
        private readonly TripDbContext _trip;
        public TripproductTable(TripDbContext trip) 
        {
            _trip = trip;
        }
        //抓商品表的那張表給自己的DTO
        public async Task<IEnumerable<TripProductDTO>> GetAllAsync()
        {
            var products = await _trip.TripProducts.Select(p => new TripProductDTO { 
              TripProductId=p.TripProductId,
              ProductName=p.ProductName,
              CoverImage=p.CoverImage,
              DisplayPrice=p.DisplayPrice
            }).ToListAsync();
            return products;
        }
    }
}
