namespace TravelWeb_API.Models.Itinerary.Service
{
    public interface ICloudinaryService
    {
        public Task<string?> UploadImageAsync(IFormFile file, string folderName);
    }
}
