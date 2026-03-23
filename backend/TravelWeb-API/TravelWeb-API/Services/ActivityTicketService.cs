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
                .AsNoTracking()
                .Where(t => t.ProductCode == productCode)
                .Select(t => new ActivityTicketInfoResponseDTO
                {
                    ProductCode = t.ProductCode,
                    ProductName = t.ProductName,
                    Description = t.ActivityTicketDetail!.ProdcutDescription,
                    TermsOfService = t.ActivityTicketDetail.TermsOfService,
                    CoverImageUrl = t.CoverImageUrl,
                    Notes = t.ActivityTicketDetail.Note,
                    TicketCategoryId = t.TicketCategory!.TicketCategoryId,
                    StartDate = t.StartDate,
                    ExpiryDate = t.ExpiryDate,
                    CurrentPrice = t.CurrentPrice,
                    //TODO
                    TicketStock = 0, //待連結庫存欄位
                })
                .FirstOrDefaultAsync();

            return result;
        }

        public async Task<List<ActivityCardReponseDTO>?> GetProductSuggestion(int activityId,List<string> activityType)
        {
            var typeCheck = await _dbContext.TagsActivityTypes.AnyAsync(t => activityType.Contains(t.ActivityType!));

            if (!typeCheck) return null;

            var result = await _dbContext.Activities
                .Where(a => a.SoftDelete == false && a.Types.Any(t => activityType.Contains(t.ActivityType!)) && a.ActivityId != activityId)
                .Where(a=>a.EndTime >= DateOnly.FromDateTime(DateTime.Today))
                .Select(a => new ActivityCardReponseDTO {
                    ActivityId = a.ActivityId,
                    Title = a.Title,
                    Type = a.Types.Select(t => t.ActivityType),
                    Region = a.Regions.Select(r => r.RegionName),
                    Start = a.StartTime,
                    End = a.EndTime,
                    CoverImageUrl = a.ActivityImages.FirstOrDefault(i => i.IsCoverImage==true)!.ImageUrl,
                    ViewCount = a.ViewCount,
                    CommentCount = a.Reviews.Count(),
                    ReferencePrice = a.ActivityTicketDetails
                    .Where(d => d.ProductCodeNavigation.TicketCategoryId == 2)
                    .Select(d => d.ProductCodeNavigation.CurrentPrice)
                    .FirstOrDefault() ?? 0,
                    AverageRating = (float)Math.Round((a.Reviews.Any() ? a.Reviews.Average(r => r.Rating) : 0), 1),
                })
                .OrderByDescending(a => a.AverageRating)
                .ThenByDescending(a=>a.ViewCount)
                .ToListAsync();

            return result;
        }
    }
}
