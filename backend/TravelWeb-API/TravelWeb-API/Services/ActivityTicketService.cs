using Microsoft.EntityFrameworkCore;
using TravelWeb_API.DTO.ActivityDTO;
using TravelWeb_API.Models.ActivityModel;
using TravelWeb_API.Models.TripProduct;

namespace TravelWeb_API.Services
{
    public class ActivityTicketService
    {
        private readonly ActivityDbContext _dbContext;

        public ActivityTicketService(ActivityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ActivityTicketInfoResponseDTO?> GetTicketInfo(string productCode)
        {
            var ticket = await _dbContext.AcitivityTickets.AnyAsync(t => t.ProductCode == productCode);
            if (!ticket) return null;

            var result = await _dbContext.AcitivityTickets
                .Where(t => t.ProductCode == productCode)
                .Select(t => new ActivityTicketInfoResponseDTO
                {
                    ProductCode = t.ProductCode,
                    ProductName = t.ProductName,
                    Description = t.ActivityTicketDetail!.ProdcutDescription,
                    TermsOfService = t.ActivityTicketDetail.TermsOfService,
                    CoverImageUrl = t.CoverImageUrl,
                    StartDate = t.StartDate,
                    ExpiryDate = t.ExpiryDate,
                    CurrentPrice = t.CurrentPrice,
                    TicketStock = 0, //待連結庫存欄位
                    Notes = null,//待新增對應欄位
                })
                .FirstOrDefaultAsync();

            return result;
        }

        public async Task<List<ActivityCardReponseDTO>?> GetProductSuggestion(string activityType)
        {
            var typeCheck = await _dbContext.TagsActivityTypes.AnyAsync(t => t.ActivityType == activityType);

            if (!typeCheck) return null;

            var result = await _dbContext.Activities
                .Where(a => a.SoftDelete == false && a.Types.Any(t => t.ActivityType == activityType))
                .OrderByDescending(a => a.StartTime) //這邊往後要改成以閱覽數當排序基準
                .Select(a => new ActivityCardReponseDTO {
                    ActivityId = a.ActivityId,
                    Title = a.Title,
                    Type = a.Types.Select(t => t.ActivityType),
                    Region = a.Regions.Select(r => r.RegionName),
                    Start = a.StartTime,
                    End = a.EndTime,
                    CoverImageUrl = a.ActivityImages.FirstOrDefault(i => i.IsCoverImage==true)!.ImageUrl,
                })
                .ToListAsync();

            return result;
        }
    }
}
