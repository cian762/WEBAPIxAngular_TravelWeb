using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TravelWeb_API.DTO.ActivityDTO;
using TravelWeb_API.Services;

namespace TravelWeb_API.Controllers.Activity
{

    [Route("api/[controller]")]
    [ApiController]
    public class PersonalReviewController : ControllerBase
    {
        private readonly ActivityReviewService _activityReviewService;
        private readonly CloudinaryPhotoService _cloudinaryPhotoService;

        public PersonalReviewController(ActivityReviewService activityReviewService, CloudinaryPhotoService cloudinaryPhotoService)
        {
            _activityReviewService = activityReviewService;
            _cloudinaryPhotoService = cloudinaryPhotoService;
        }

        [HttpGet("{activityId}")]
        public async  Task<ActionResult> GetPersonalReviews([FromRoute]int activityId)
        {
            var memberCode = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

            var result = await _activityReviewService.GetPersonalReviews(activityId,memberCode);
            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> PostPersonalReview([FromForm] ReviewRequestDTO request)
        {
            var memberCode = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

            var imageDetails = new List<ImageUploadResult>();

            if (request.ReviewImages !=null &&request.ReviewImages.Any()) 
            {
                imageDetails = await _cloudinaryPhotoService.AddPhotoAsync(request.ReviewImages);
            }
            

            var result = await _activityReviewService.PostPersonalReview(request, memberCode, imageDetails);
            
            return Ok(new { message = "新增成功", data = result});
        }

        [Authorize]
        [HttpPatch]
        public async Task<ActionResult> PatchPersonalReview([FromForm]ReviewEditRequestDTO request)
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
            return Ok(new { message = "修改成功", data = result });
        }

        [Authorize]
        [HttpDelete]
        public async Task<ActionResult> DeletePersonalReview([FromQuery]int reviewId) 
        {
            var result = await _activityReviewService.DeletePersonalReview(reviewId);
            return Ok(new { message = "刪除成功", data = result });
        }
    }
}
