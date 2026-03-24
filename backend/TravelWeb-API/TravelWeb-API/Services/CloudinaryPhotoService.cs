using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace TravelWeb_API.Services
{
    public class CloudinaryPhotoService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryPhotoService(IConfiguration _config)
        {

            string? cloudName = _config["CloudinaryGFC:CloudName"];
            string? apiKey = _config["CloudinaryGFC:ApiKey"];
            string? apiSecret= _config["CloudinaryGFC:ApiSecret"];

            if (string.IsNullOrWhiteSpace(cloudName) ||string.IsNullOrWhiteSpace(apiKey) ||string.IsNullOrWhiteSpace(apiSecret))
            {
                throw new InvalidOperationException("CloudinaryGFC 設定缺少必要欄位。");
            }

            var acc = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(acc);
        }

        //新增照片
        public async Task<List<ImageUploadResult>> AddPhotoAsync(List<IFormFile> file)
        {
            var uploadResult = new ImageUploadResult();
            var resultCollection = new List<ImageUploadResult>();

            if (file != null && file.Any())
            {
                foreach (var f in file)
                {
                    using var stream = f.OpenReadStream();
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(f.FileName, stream),
                        Transformation = new Transformation().Width(1000).Crop("fit")
                    };
                    uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    resultCollection.Add(uploadResult);
                }
            }
            return resultCollection;
        }


        //刪除照片
        public async Task DeletePhotoAsync(List<string>? publicIds)
        {

            if (publicIds == null || !publicIds.Any()) return;

            var delParams = new DelResParams()
            {
                PublicIds = publicIds,
                Type = "upload",
                ResourceType = ResourceType.Image
            };

            var result = await _cloudinary.DeleteResourcesAsync(delParams);

        }
    }
}
