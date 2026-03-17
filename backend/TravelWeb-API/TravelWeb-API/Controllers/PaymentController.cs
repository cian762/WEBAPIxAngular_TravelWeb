using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelWeb_API.Models.TripProduct.ITripProduct;

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IECPay _ecpayService;
        private readonly IOrder _orderService;
        public PaymentController(IECPay ecpayService, IOrder orderService)
        {
            _ecpayService = ecpayService;
            _orderService = orderService;
        }
       
    }
}
