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
                .Where(o => o.OrderStatus == "已付款")
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
                
            if (check == null) return;

            var qrCodeEntities = check.OrderItems
                .Select(i => new QrcodeInfo
                { 
                    OrderId = i.OrderId,
                    Qrtoken = _qrCodeService.GenerateToken(),
                    Status = "Unused",
                    CreateAt = DateTime.Now
                    //往後再決定要不要加上expiredDate;
                })
                .ToList();

            _dbcontext.QrcodeInfos.AddRange(qrCodeEntities);
           
            await _dbcontext.SaveChangesAsync();
        }
    }
}
