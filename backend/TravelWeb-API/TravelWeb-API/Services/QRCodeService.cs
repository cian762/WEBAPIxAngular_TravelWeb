using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Options;
using QRCoder;
using System.Security.Cryptography;
using TravelWeb_API.Models.TripProduct;

namespace TravelWeb_API.Services
{
    public class QRCodeService
    {
        private readonly QrCodeSettings _settings;

        public QRCodeService(IOptions<QrCodeSettings> options)
        {
            _settings = options.Value;
        }

        //產生 token
        public string GenerateToken()
        {
            byte[] randomBytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToHexString(randomBytes).ToLower();
        }

        //建立要放進QRCode 裡的東西，放驗證票券時要打到的 api 路徑
        public string BuildVerifyUrl(string token) 
        {
            if(string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException("token不可為空", nameof(token));

            if (string.IsNullOrWhiteSpace(_settings.VerifyBaseUrl))
                throw new InvalidOperationException("QrCodeSettings:VerifyBaseUrl 尚未設定");

            return $"{_settings.VerifyBaseUrl}?token={Uri.EscapeDataString(token)}";

        }

        // QR code 轉換成照片
        public byte[] GenerateQrPngBytes(string content) 
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentNullException("QR 內容不可為空", nameof(content));
            using var qrGenerator = new QRCodeGenerator();
            
            //依據內容產出 QRCodeData，ECCLevel 是錯誤修正等級(容錯率 L 最低, H 最高)
            using QRCodeData qrCodeData = qrGenerator.CreateQrCode(
                content,QRCodeGenerator.ECCLevel.Q);
            
            //輸出 QrCode PNG 圖片
            var pngQrCode = new PngByteQRCode(qrCodeData);
            //pixelPerModule 越大，圖越大
            return pngQrCode.GetGraphic(20);
        }


        //將 QRcode 顯示在前端畫面上使用
        public string GenerateQrBase64(string content) 
        {
            byte[] pngBytes = GenerateQrPngBytes(content);
            return Convert.ToBase64String(pngBytes);
        }
    }

    public class QrCodeSettings 
    {
        public string VerifyBaseUrl { get; set; } = string.Empty;
    }
}
