using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models.TripProduct;

namespace TravelWeb_API.Services
{
    public class TicketService
    {
        private readonly TripDbContext _dbcontext;
        private readonly QRCodeService _qrCodeService;

        public TicketService(TripDbContext dbcontext, QRCodeService qrCodeService)
        {
            _dbcontext = dbcontext;
            _qrCodeService = qrCodeService;
        }

        //將成功付款訂單中的商品明細，轉換成 QRCode Token 寫進資料庫
        public async Task CreateQrCodeForOrderAsync(int orderId) 
        {
            var check = await _dbcontext.Orders
                .Include(o=>o.OrderItems)
                .ThenInclude(oi=>oi.OrderItemTickets)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.PaymentStatus == "已付款");

            if (check == null ) return;

            var target = check.OrderItems.Where(oi => !string.IsNullOrWhiteSpace(oi.ProductCode) &&
                 (oi.ProductCode.StartsWith("ACT-") || oi.ProductCode.StartsWith("TKT-")))
                .ToList();

            var qrCodeEntities = new List<QrcodeInfo>();
            foreach (var item in target) 
            {
                var quantity = item.OrderItemTickets.Sum(ot => ot.Quantity);
                
                for (int i = 0; i < quantity; i++) 
                {
                    qrCodeEntities.Add(new QrcodeInfo
                    {
                        OrderId = orderId,
                        OrderItemId = item.OrderItemId,
                        Qrtoken = _qrCodeService.GenerateToken(),
                        Status = "Unused",
                        CreateAt = DateTime.Now
                    });
                }
            }
                
            _dbcontext.QrcodeInfos.AddRange(qrCodeEntities);
           
            await _dbcontext.SaveChangesAsync();
        }
    }
}
