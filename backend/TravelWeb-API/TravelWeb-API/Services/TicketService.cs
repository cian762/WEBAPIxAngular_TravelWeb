using Microsoft.EntityFrameworkCore;
using TravelWeb_API.DTO.ActivityDTO;
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
        public void CreateQrCodeForOrderAsync(int orderId) 
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
           
            _dbcontext.SaveChanges();
        }


        //此服務只提供狀態檢查，不改狀態
        public async Task<QrCodeValidationResponesDTO> ValidateQrCodeAsync(string token, string? operatorId = null) 
        {
            //如果傳進來的 Token 為空則直接結束 function
            if (string.IsNullOrWhiteSpace(token)) 
            {
                return new QrCodeValidationResponesDTO
                {
                    IsSuccess = false,
                    Message = "QRCode Token 不可為空"

                };
            
            }

            var qr = await _dbcontext.QrcodeInfos
                .Include(q=>q.Order)
                .FirstOrDefaultAsync(q=>q.Qrtoken == token);

            //如果資料庫沒有撈到此 Token 對應的資料就走 if 判斷式內部
            if (qr == null) 
            {
                await WriteVerificationLogAsync(null, "ValFailed", "查無此票券",operatorId);

                return new QrCodeValidationResponesDTO
                {
                    IsSuccess = false,
                    Message = "查無此票券"

                };
            }


            //檢查票券是否有期限，有沒有過期
            if (qr.ExpiredDate.HasValue && qr.ExpiredDate.Value < DateOnly.FromDateTime(DateTime.Now)) 
            {
                await WriteVerificationLogAsync(qr.QrcodeId, "ValFailed", "票券已過期", operatorId);
                return new QrCodeValidationResponesDTO
                {
                    IsSuccess = false,
                    Message = "票券已過期",
                    QrcodeId = qr.QrcodeId,
                    QrToken = qr.Qrtoken,
                    Status = qr.Status,
                    ExpiredDate = qr.ExpiredDate,
                    UseAt = qr.UsedAt,
                };
            }

            //檢查票券是否已經使用過了
            if (qr.Status == "Used") 
            {
                await WriteVerificationLogAsync(qr.QrcodeId, "ValFailed", "票券已使用", operatorId);

                return new QrCodeValidationResponesDTO
                {
                    IsSuccess = false,
                    Message = "票券已使用",
                    QrcodeId=qr.QrcodeId,
                    QrToken = qr.Qrtoken,
                    Status = qr.Status,
                    ExpiredDate = qr.ExpiredDate,
                    UseAt = qr.UsedAt,
                    ProductCode = qr.Order.ProductCode ?? string.Empty,
                    ProductName = qr.Order.ProductNameSnapshot ?? string.Empty
                };
            }

            //檢查票券狀態是否不等於未使用，若為未使用以外的狀態，皆不准使用
            if (qr.Status != "Unused") 
            {
                await WriteVerificationLogAsync(qr.QrcodeId, "ValFailed", $"票券狀態不可使用:{qr.Status}", operatorId);

                return new QrCodeValidationResponesDTO
                {
                    IsSuccess = false,
                    Message = $"票券狀態不可使用:{qr.Status}",
                    QrcodeId = qr.QrcodeId,
                    QrToken= qr.Qrtoken,
                    Status = qr.Status,
                    ExpiredDate = qr.ExpiredDate,
                    UseAt = qr.UsedAt,
                };
            }


            //上面排除所有不能用的可能性後，下方開始抓可以用的票券資訊
            string? productName = null;
            string? productCode = null;

            var orderitem = await _dbcontext.OrderItems
                .Where(oi => oi.OrderItemId == qr.OrderItemId)
                .Select(oi => new
                {
                    oi.ProductNameSnapshot,
                    oi.ProductCode,
                })
                .FirstOrDefaultAsync();
            productName = orderitem?.ProductNameSnapshot;
            productCode = orderitem?.ProductCode;

            await WriteVerificationLogAsync(qr.QrcodeId, "ValSuccess", "票券可使用", operatorId);

            var verifyUrl = _qrCodeService.BuildVerifyUrl(qr.Qrtoken);
            byte[] qrImageBytes = _qrCodeService.GenerateQrPngBytes(verifyUrl);


            return new QrCodeValidationResponesDTO
            {
                IsSuccess = true,
                Message = "票券可使用",
                QrcodeId = qr.QrcodeId,
                QrToken = qr.Qrtoken,
                Status = qr.Status,
                ExpiredDate = qr.ExpiredDate,
                UseAt = qr.UsedAt,
                ProductName = productName ?? string.Empty,
                ProductCode = productCode ?? string.Empty,
                QRCodeImage = qrImageBytes
            };
        }


        public async Task<QrCodeValidationResponesDTO> RedeemQrCodeAsync(string token, string? operatorId = null)
        {

            if (string.IsNullOrWhiteSpace(token))
            {
                return new QrCodeValidationResponesDTO
                {
                    IsSuccess = false,
                    Message = "QRCode Token 不可為空"
                };
            }

            var qr = await _dbcontext.QrcodeInfos
                .FirstOrDefaultAsync(q => q.Qrtoken == token);

            if (qr == null)
            {
                await WriteVerificationLogAsync(null, "RedFailed", "查無此票券", operatorId);


                return new QrCodeValidationResponesDTO
                {
                    IsSuccess = false,
                    Message = "查無此票券"
                };
            }

            if (qr.ExpiredDate.HasValue && qr.ExpiredDate.Value < DateOnly.FromDateTime(DateTime.Now))
            {
                await WriteVerificationLogAsync(qr.QrcodeId, "RedFailed", "票券已過期", operatorId);

                return new QrCodeValidationResponesDTO
                {
                    IsSuccess = false,
                    Message = "票券已過期"
                };
            }

            if (qr.Status == "Used")
            {
                await WriteVerificationLogAsync(qr.QrcodeId, "RedFailed", "票券已使用", operatorId);

                return new QrCodeValidationResponesDTO
                {
                    IsSuccess = false,
                    Message = "票券已使用",
                    QrcodeId = qr.QrcodeId,
                    QrToken = qr.Qrtoken,
                    Status = qr.Status,
                    UseAt = qr.UsedAt
                };
            }

            if (qr.Status != "Unused")
            {
                await WriteVerificationLogAsync(qr.QrcodeId, "RedFailed", $"票券狀態不可核銷：{qr.Status}", operatorId);

                return new QrCodeValidationResponesDTO
                {
                    IsSuccess = false,
                    Message = $"票券狀態不可核銷：{qr.Status}"
                };
            }

            qr.Status = "Used";
            qr.UsedAt = DateTime.Now;

            //這是要記錄工作人員的ID，不是紀錄使用者是誰
            qr.UsedBy = operatorId;

            await _dbcontext.SaveChangesAsync();


            await WriteVerificationLogAsync(qr.QrcodeId, "RedSuccess", "票券核銷成功", operatorId);

            string? productName = null;
            string? productCode = null;

            var orderItem = await _dbcontext.OrderItems
                .Where(oi => oi.OrderItemId == qr.OrderItemId)
                .Select(oi => new
                {
                    oi.ProductNameSnapshot,
                    oi.ProductCode
                })
                .FirstOrDefaultAsync();
            
            productName = orderItem?.ProductNameSnapshot;
            productCode = orderItem?.ProductCode;

            return new QrCodeValidationResponesDTO
            {
                IsSuccess = true,
                Message = "票券核銷成功",
                QrcodeId = qr.QrcodeId,
                QrToken = qr.Qrtoken,
                Status = qr.Status,
                ExpiredDate = qr.ExpiredDate,
                UseAt = qr.UsedAt,
                ProductName = productName ?? string.Empty,
                ProductCode = productCode ?? string.Empty,
            };

        }





        private async Task WriteVerificationLogAsync(int? qrCodeId, string action, string message, string? operatorId) 
        {
            var log = new QrcodeVerification
            {
                QrcodeId = qrCodeId ?? 0,
                Action = action,
                Message = message,
                CreatedAt = DateTime.Now,
                OperatorId = operatorId ?? "測試用工作人員標號",
            };

            _dbcontext.QrcodeVerifications.Add(log);
            await _dbcontext.SaveChangesAsync();
        }

    }
}
