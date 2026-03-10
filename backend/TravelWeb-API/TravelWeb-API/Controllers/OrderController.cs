using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelWeb_API.Models.TripProduct;

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly TripDbContext _trip;
        public OrderController(TripDbContext trip) {
            _trip = trip;

        }

        [HttpGet]
        public IActionResult Test ()
        {
            return Ok( _trip.TripProducts.FirstOrDefault());
        }




    }
}
