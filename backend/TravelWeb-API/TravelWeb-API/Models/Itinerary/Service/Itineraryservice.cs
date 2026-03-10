using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models.Itinerary.DBContext;
using TravelWeb_API.Models.Itinerary.DBModel;
using TravelWeb_API.Models.Itinerary.DTO;
namespace TravelWeb_API.Models.Itinerary.Service
{
    public class ItineraryService : IItineraryservice
    {
        private readonly TravelContext _context;
        public ItineraryService(TravelContext context) => _context = context;

        public async Task<int> CreateItineraryWithItemsAsync(ItineraryCreateDto dto)
        {
            // 啟動資料庫交易
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. 建立行程主表
                var newItinerary = new DBModel.Itinerary
                {
                    MemberId = dto.MemberId,
                    ItineraryName = dto.ItineraryName,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    CreateTime = DateTime.Now,
                    Introduction = dto.Introduction
                };
                _context.Itineraries.Add(newItinerary);
                await _context.SaveChangesAsync(); // 先存一次拿 ID
                var initialVersion = new ItineraryVersion
                {
                    ItineraryId = newItinerary.ItineraryId,
                    VersionNumber = 1, // 或者叫 v1.0
                    Creator = 1,
                    CreateTime = DateTime.Now,
                    Source = "Manual", // 標記來源是手動建立
                    CurrentUsageStatus = "Y"    // 標記為當前使用版本
                };
                _context.ItineraryVersions.Add(initialVersion);
                await _context.SaveChangesAsync(); // 取得 VersionID
                // 2. 處理景點 (ItemsToPush)
                int currentOrder = 100;
                foreach (var input in dto.ItemsToPush)
                {
                    int finalAttractionId;

                    // 情況 A：使用者選的是 DB 既有的景點
                    if (input.AttractionId.HasValue)
                    {
                        finalAttractionId = input.AttractionId.Value;
                    }
                    // 情況 B：使用者從 Google 找的地點
                    else if (input.ExternalLocation != null)
                    {
                        // 檢查 DB 是否已有此 Google 地點
                        var existing = await _context.Attractions
                            .FirstOrDefaultAsync(a => a.GooglePlaceId == input.ExternalLocation.GooglePlaceId);

                        if (existing != null)
                        {
                            finalAttractionId = existing.AttractionId;
                        }
                        else
                        {
                            // DB 沒看過，新增一筆 Attraction
                            var newAttr = new Attraction
                            {
                                Name = input.ExternalLocation.Name,
                                RegionId = 1000,  //待定
                                Address = input.ExternalLocation.Address,
                                GooglePlaceId = input.ExternalLocation.GooglePlaceId,
                                Latitude = input.ExternalLocation.Latitude,
                                Longitude = input.ExternalLocation.Longitude,
                                CreatedAt = DateTime.Now,
                                IsDeleted = false
                            };
                            _context.Attractions.Add(newAttr);
                            await _context.SaveChangesAsync(); // 取得新產生的 ID
                            finalAttractionId = newAttr.AttractionId;
                        }
                    }
                    else continue; // 兩者皆無則跳過

                    // 3. 建立 ItineraryItems 關聯
                    var newItem = new ItineraryItem
                    {
                        VersionId = initialVersion.VersionId,
                        AttractionId = finalAttractionId,
                        SortOrder = currentOrder,
                        ContentDescription = input.UserNote ?? "自訂行程項目",
                        DayNumber = 1 // 初始預設第一天
                    };
                    _context.ItineraryItems.Add(newItem);
                    currentOrder += 100;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync(); // 全部成功，提交交易
                return newItinerary.ItineraryId;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(); // 任何一步失敗，全部撤回
                throw;
            }
        }
    }
}
