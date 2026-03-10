using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelWeb_API.Models.attraction;

namespace TravelWeb_API.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class AttractionController : ControllerBase
    {
        private readonly AttractionsContext _dbContext;

        public AttractionController(AttractionsContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult GetAttraction() {

            return Ok(_dbContext.Attractions.FirstOrDefault());
        }

    }
}
