using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis;
using System.IdentityModel.Tokens.Jwt;
using TravelWeb_API.DTO.ActivityDTO;
using TravelWeb_API.Services;

namespace TravelWeb_API.Controllers.Activity
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityReviewController : ControllerBase
    {
        //private readonly string _memberId;
        private readonly ActivityReviewService _activityReviewService;
        private readonly CloudinaryPhotoService _cloudinaryPhotoService;

        public ActivityReviewController(ActivityReviewService activityReviewService, CloudinaryPhotoService cloudinaryPhotoService)
        {
            //_memberId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value!;
            _activityReviewService = activityReviewService;
            _cloudinaryPhotoService = cloudinaryPhotoService;
        }

        [HttpGet]
        public async  Task<ActionResult> GetPersonalReviews(int activityId,string memberId2)
        {
            var result = await _activityReviewService.GetPersonalReviews(activityId,memberId2);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> PostPersonalReview(ReviewRequestDTO request, string memberId2)
        {
            var imageDetails = new List<ImageUploadResult>();

            if (request.ReviewImages !=null &&request.ReviewImages.Any()) 
            {
                imageDetails = await _cloudinaryPhotoService.AddPhotoAsync(request.ReviewImages);
            }
            
            var result = await _activityReviewService.PostPersonalReview(request,memberId2,imageDetails);
            
            return Ok(result);
        }

        [HttpPatch]
        public async Task<ActionResult> PatchPersonalReview(ReviewEditRequestDTO request)
        {
            var imageDetails = new List<ImageUploadResult>();

            //修改時，新增圖片，imageDetails 變數型別為 List<ImageUploadResult>，ImageUploadResult 類別上有 imageUrl和 PublicId 資訊
            if (request.NewImages != null && request.NewImages.Any())
            {
                imageDetails = await _cloudinaryPhotoService.AddPhotoAsync(request.NewImages);
            }

            //修改時，刪除圖片，透過 imgurl 找到 publicID 資訊，把雲端圖床上照片刪除
            if (request.DeletedImageUrls != null && request.DeletedImageUrls.Any()) 
            {
                var publicIds = _activityReviewService.FindPublicId(request.DeletedImageUrls);
                await _cloudinaryPhotoService.DeletePhotoAsync(publicIds);
            }

            var result = await _activityReviewService.PatchPersonalReview(request,imageDetails);
            return Ok(result);
        }


        [HttpDelete]
        public async Task<ActionResult> DeletePersonalReview(int reviewId) 
        {
            var result = await _activityReviewService.DeletePersonalReview(reviewId);
            return Ok(result);
        }
    }
}
