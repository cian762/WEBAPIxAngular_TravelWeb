using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace TravelWeb_API.Models.Itinerary.Service
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            // 從 appsettings.json 讀取設定
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }
        public async Task<string?> UploadImageAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0) return null;

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folderName, // 建議按功能分資料夾，例如 "itineraries"
                                     // 可選：在儲存時自動進行裁剪或壓縮
                Transformation = new Transformation().Width(800).Height(600).Crop("fill")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return uploadResult.SecureUrl.ToString(); // 回傳 https 連結
            }
            Console.WriteLine(uploadResult.Error?.Message);
            return null;
        }
        public async Task<DeletionResult> DeletePhotoAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);

            // 呼叫 Cloudinary API 執行刪除
            var result = await _cloudinary.DestroyAsync(deleteParams);

            return result;
        }
    }
}
