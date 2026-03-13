using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelWeb_API.Models.TripProduct;
using TravelWeb_API.Models.TripProduct.ITripProduct;

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrder _order;
        public OrderController(IOrder order) {
            _order = order;

        }

     



    }
}
