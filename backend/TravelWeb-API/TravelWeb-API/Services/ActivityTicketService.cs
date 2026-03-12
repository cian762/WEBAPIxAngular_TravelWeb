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

        public ActivityTicketInfoResponseDTO? GetTicketInfo(string productCode)
        {
            var ticket = _dbContext.AcitivityTickets.Any(t => t.ProductCode == productCode);
            if (!ticket) return null;

            var result = _dbContext.AcitivityTickets
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
                }).FirstOrDefault();

            return result;
        }

        public List<ActivityCardReponseDTO>? GetProductSuggestion(string activityType)
        {
            var typeCheck = _dbContext.TagsActivityTypes.Any(t => t.ActivityType == activityType);

            if (!typeCheck) return null;

            var result = _dbContext.Activities
                .Where(a => a.SoftDelete == false && a.Types.Any(t => t.ActivityType == activityType))
                .Select(a => new ActivityCardReponseDTO {
                    ActivityId = a.ActivityId,
                    Title = a.Title,
                    Type = a.Types.Select(t => t.ActivityType),
                    Region = a.Regions.Select(r => r.RegionName),
                    Start = a.StartTime,
                    End = a.EndTime,
                    //CoverImageUrl = a.ActivityImages.FirstOrDefault(i => i.IsCoverImage==true).ImageUrl,
                })
                .OrderByDescending(a => a.Start) //這邊往後要改成以閱覽數當排序基準
                .ToList();

            return result;
        }
    }
}
