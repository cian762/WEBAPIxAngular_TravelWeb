using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TravelWeb_API.DTO.ActivityDTO;
using TravelWeb_API.Models.ActivityModel;
using TravelWeb_API.Models.Board.DbSet;

namespace TravelWeb_API.Services
{
    public class ActivityReviewService
    {
        private readonly ActivityDbContext _activityDbContext;
        public ActivityReviewService(ActivityDbContext activityDbContext)
        {
            _activityDbContext = activityDbContext;
        }

        public async Task<List<ReviewResponseDTO>> GetPersonalReviews(int activityId, string memberId) 
        {

            var result = await _activityDbContext.Activities
                .Where(a => a.ActivityId == activityId)
                .SelectMany(a => a.Reviews)
                .Where(r => r.MemberId == memberId && r.IsSoftDeleted == false)
                .Select(r => new ReviewResponseDTO
                {
                    ReviewId = r.ReviewId,
                    MemberId = r.MemberId,
                    Title = r.Title,
                    Comment = r.Comment,
                    Rating = r.Rating,
                    CreateDate = r.CreateDate,
                    ReviewImages = r.ReviewImages.Select(i => i.ImageUrl).ToList()!
                })
                .ToListAsync();
                
            return result;
        }


        public async Task<string> PostPersonalReview(ReviewRequestDTO request,string memberId2,List<ImageUploadResult> imageDetails) 
        {
            var selectActivity = await _activityDbContext.Activities
                .Where(a => a.ActivityId == request.ActivityId)
                .FirstOrDefaultAsync();

            if (selectActivity != null) 
            {
                var comment = new ProductReview()
                {
                    MemberId = memberId2,
                    Title = request.Title,
                    Comment = request.Comment,
                    Rating = request.Rating,
                    CreateDate = DateOnly.FromDateTime(DateTime.Today)
                };
                
                if (imageDetails != null && imageDetails.Any()) 
                {
                    foreach (var image in imageDetails) 
                    {
                        comment.ReviewImages.Add(new ReviewImage
                        {
                            PublicId = image.PublicId,
                            ImageUrl = image.SecureUrl.AbsoluteUri,
                        });
                    }
                }

                comment.Activities.Add(selectActivity);
                _activityDbContext.ProductReviews.Add(comment);
                _activityDbContext.SaveChanges();
                return comment.Title + "新增評論成功";
            }

            return "新增評論失敗";
        }


        public async Task<string> PatchPersonalReview(ReviewEditRequestDTO request,List<ImageUploadResult> imageDetails) 
        {
            var check = await _activityDbContext.ProductReviews
                .Include(r=>r.ReviewImages)
                .FirstOrDefaultAsync(r => r.ReviewId == request.ReviewId);

            if (check == null) return "修改失敗，找無此評論內容";

            check.Title = request.Title;
            check.Comment = request.Comment;
            check.Rating = request.Rating;

            // 添加新照片資訊進資料庫
            if (imageDetails != null && imageDetails.Any())
            {
                foreach (var image in imageDetails)
                {
                    check.ReviewImages.Add(new ReviewImage
                    {
                        PublicId = image.PublicId,
                        ImageUrl = image.SecureUrl.AbsoluteUri,
                    });
                }
            }

            // 移除舊照片資料
            if (request.DeletedImageUrls != null && request.DeletedImageUrls.Any()) 
            {
                foreach (var url in request.DeletedImageUrls) 
                {
                    var photoToRemove = _activityDbContext.ReviewImages.FirstOrDefault(u => u.ImageUrl == url);

                    if (photoToRemove != null) {
                    
                        _activityDbContext.ReviewImages.Remove(photoToRemove);
                    }
                }
            }

            check.CreateDate = DateOnly.FromDateTime(DateTime.Today);
            await _activityDbContext.SaveChangesAsync();
            return $"{check.Title}，修改成功" ;
        }


        public async Task<string> DeletePersonalReview(int reviewId) 
        {
            var check = await _activityDbContext.ProductReviews.FirstOrDefaultAsync(r => r.ReviewId == reviewId);
            if (check == null) return "無法刪除評論，找不到對應評論ID";

            check.IsSoftDeleted = true;
            await _activityDbContext.SaveChangesAsync();
            return $"已刪除此評論, 發生時間 {DateTime.Now}";
        }


        public List<string> FindPublicId(List<string> deletedImageUrls) 
        {
            var result = _activityDbContext.ReviewImages
                .Where(i => deletedImageUrls.Contains(i.ImageUrl))
                .Select(i=>i.PublicId)
                .ToList();

            return result;
        }

    }
}
