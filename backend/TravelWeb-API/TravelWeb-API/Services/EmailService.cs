using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Utils;
using Newtonsoft.Json.Linq;
using TravelWeb_API.Models.TripProduct;

namespace TravelWeb_API.Services
{
    public class EmailService
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly QRCodeService _qrCodeService;
        private readonly TripDbContext _dbcontext;

        public EmailService(IOptions<SmtpSettings> smtpSettings, QRCodeService qrCodeService, TripDbContext dbcontext)
        {
            _smtpSettings = smtpSettings.Value;
            _qrCodeService = qrCodeService;
            _dbcontext = dbcontext;
        }

        public async Task SendOrderTicketEmailAsync(int orderId)
        {
            var order = await _dbcontext.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return;

            var orderItems = order.OrderItems
                .Select(oi => new
                {
                    ProductCode = oi.ProductCode,
                    ProductNameSnapshot = oi.ProductNameSnapshot,
                    QRcodeId = oi.QrcodeInfos.Select(q => q.QrcodeId).First(),
                    QRToken = oi.QrcodeInfos.Select(q => q.Qrtoken).First(),
                    ExpiredDate = oi.QrcodeInfos.Select(q => q.ExpiredDate).First(),
                });

            //開始寫 Email 區塊
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromEmail));
            message.To.Add(MailboxAddress.Parse(order.ContactEmail ?? "chenguanfu4@gmail.com"));
            message.Subject = $"您的電子票券 - 訂單編號 #{order.OrderId}";

            var builder = new BodyBuilder();

            var html = $@"
                <h2>您好，您的訂單已完成</h2>
                <p>訂單編號：{order.OrderId}</p>
                <p>以下是您的電子票券：</p>
            ";

            int index = 1;

            foreach (var ticket in orderItems) 
            {
                string verifyUrl = _qrCodeService.BuildVerifyUrl(ticket.QRToken);
                byte[] qrImageBytes = _qrCodeService.GenerateQrPngBytes(verifyUrl);

                //把 QRcode PNG 嵌入到 Email 中
                var image = builder.LinkedResources.Add($"qrcode_{ticket.QRcodeId}.png",qrImageBytes);
                image.ContentId = MimeUtils.GenerateMessageId();

                string expireText = ticket.ExpiredDate?.ToString() ?? "無限期使用";

                html += $@"
                    <hr />
                    <h3>票券 {index}</h3>
                    <p>商品名稱：{ticket.ProductNameSnapshot}</p>
                    <p>票券代碼：{ticket.QRToken}</p>
                    <p>使用期限：{expireText}</p>
                    <p><img src=""cid:{image.ContentId}"" alt=""QR Code"" style=""width:220px; height:220px;"" /></p>
                    <p>若無法顯示圖片，請聯絡客服。</p>
                ";

                index++;
            }

            html += @"
                <hr />
                <p>請於入場時出示此 QR Code。</p>
                <p>謝謝您的購買。</p>
            ";

            builder.HtmlBody = html;
            message.Body = builder.ToMessageBody();

            try
            {

                //啟動 SMTP 寄信
                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_smtpSettings.UserName, _smtpSettings.Password);
                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new Exception("寄送票券郵件失敗" + ex.Message, ex);
            }

        }
    }

    public class SmtpSettings 
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }

    public class TicketEmailItemDto 
    {
        public int QRCodeId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string QRToken { get; set; } = string.Empty;
    }
}
