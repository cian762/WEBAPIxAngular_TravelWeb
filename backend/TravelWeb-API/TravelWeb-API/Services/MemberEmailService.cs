// 檔案：Services/MemberEmailService.cs
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Microsoft.Extensions.Configuration;

namespace TravelWeb_API.Services
{
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

            email.From.Add(new MailboxAddress(
                _config["EmailSettings:SenderName"],
                _config["EmailSettings:SenderEmail"]));

            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = "[島敘] 您的會員註冊驗證碼";

            string htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                    <h2 style='color: #00796B; text-align: center;'>歡迎註冊 島敘</h2>
                    <p style='font-size: 16px; color: #333;'>親愛的使用者您好，</p>
                    <p style='font-size: 16px; color: #333;'>感謝您註冊我們的服務！為了確認這是您本人的信箱，請在註冊頁面輸入以下 6 位數驗證碼：</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <span style='font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #1877f2; background-color: #f0f2f5; padding: 15px 30px; border-radius: 8px;'>{code}</span>
                    </div>
                    <p style='font-size: 14px; color: #999;'>※ 此驗證碼將於 10 分鐘後失效。<br>※ 如果您並未要求註冊，請忽略此信件。</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                    <p style='font-size: 12px; color: #aaa; text-align: center;'>島敘 旅遊網 敬上</p>
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

        // ==========================================
        // 🔥 請把下面這整段貼在這裡！(寄送「重設密碼」專屬驗證信)
        // ==========================================
        public async Task SendPasswordResetEmailAsync(string toEmail, string code, string resetLink)
        {
            var email = new MimeMessage();

            // 從 appsettings.json 讀取寄件人資訊
            email.From.Add(new MailboxAddress(
                _config["EmailSettings:SenderName"],
                _config["EmailSettings:SenderEmail"]));

            email.To.Add(MailboxAddress.Parse(toEmail));

            // 設定信件主旨
            email.Subject = "[Travelista] 您的重設密碼驗證信";

            // 設計超有質感的 HTML 信件內容 (包含 4 位數驗證碼與按鈕)
            string htmlBody = $@"
                <div style='font-family: ""Helvetica Neue"", Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 30px; background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 15px rgba(0,0,0,0.05); border: 1px solid #eaeaea;'>
                    
                    <h2 style='color: #5b73e8; text-align: center; margin-bottom: 20px; font-size: 24px;'>重設密碼申請</h2>
                    
                    <p style='font-size: 16px; color: #4a4a4a; line-height: 1.6;'>親愛的使用者您好，</p>
                    <p style='font-size: 16px; color: #4a4a4a; line-height: 1.6;'>我們收到了您重設 Travelista 帳號密碼的申請。請點擊下方按鈕前往重設頁面，並在頁面中輸入以下 <strong>4 位數驗證碼</strong>：</p>
                    
                    <!-- 醒目的 4 位數驗證碼 -->
                    <div style='text-align: center; margin: 35px 0;'>
                        <span style='font-size: 36px; font-weight: 800; letter-spacing: 8px; color: #5b73e8; background-color: #f0f2f5; padding: 15px 35px; border-radius: 8px; display: inline-block;'>{code}</span>
                    </div>

                    <!-- 點擊跳轉的按鈕 -->
                    <div style='text-align: center; margin: 35px 0;'>
                        <a href='{resetLink}' style='background-color: #5b73e8; color: #ffffff; text-decoration: none; padding: 14px 40px; font-size: 16px; font-weight: bold; border-radius: 50px; display: inline-block; box-shadow: 0 4px 6px rgba(91, 115, 232, 0.3);'>立即前往重設密碼</a>
                    </div>
                    
                    <p style='font-size: 14px; color: #888888; text-align: center; margin-top: 30px;'>
                        ※ 此驗證碼與連結將於 <strong>15 分鐘後失效</strong>。<br>
                        ※ 若您並未申請重設密碼，請立即忽略並刪除此信件，您的帳號安全無虞。
                    </p>
                    
                    <hr style='border: none; border-top: 1px solid #f0f0f0; margin: 30px 0;'>
                    <p style='font-size: 12px; color: #bbbbbb; text-align: center;'>Travelista 旅遊網 敬上</p>
                </div>";

            email.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

            // 執行寄信動作
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