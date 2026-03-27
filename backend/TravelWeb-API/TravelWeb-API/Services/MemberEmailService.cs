// 檔案：Services/MemberEmailService.cs
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Microsoft.Extensions.Configuration;

namespace TravelWeb_API.Services
{
    // 獨立的實作類別，完全不干涉組員原本的 EmailService
    public class MemberEmailService : IMemberEmailService
    {
        private readonly IConfiguration _config;

        public MemberEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendVerificationCodeAsync(string toEmail, string code)
        {
            var email = new MimeMessage();

            // 從 appsettings.json 的 EmailSettings 區塊讀取
            email.From.Add(new MailboxAddress(
                _config["EmailSettings:SenderName"],
                _config["EmailSettings:SenderEmail"]));

            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = "[Travelista] 您的會員註冊驗證碼";

            string htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                    <h2 style='color: #00796B; text-align: center;'>歡迎註冊 Travelista</h2>
                    <p style='font-size: 16px; color: #333;'>親愛的使用者您好，</p>
                    <p style='font-size: 16px; color: #333;'>感謝您註冊我們的服務！為了確認這是您本人的信箱，請在註冊頁面輸入以下 6 位數驗證碼：</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <span style='font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #1877f2; background-color: #f0f2f5; padding: 15px 30px; border-radius: 8px;'>{code}</span>
                    </div>
                    <p style='font-size: 14px; color: #999;'>※ 此驗證碼將於 10 分鐘後失效。<br>※ 如果您並未要求註冊，請忽略此信件。</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                    <p style='font-size: 12px; color: #aaa; text-align: center;'>Travelista 旅遊網 敬上</p>
                </div>";

            email.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(_config["EmailSettings:SmtpServer"], int.Parse(_config["EmailSettings:SmtpPort"]), SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_config["EmailSettings:SenderEmail"], _config["EmailSettings:SenderPassword"]);
                await smtp.SendAsync(email);
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }
    }
}