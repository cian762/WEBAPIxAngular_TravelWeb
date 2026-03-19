using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models.Itinerary.DBContext;
using TravelWeb_API.Models.Itinerary.DBModel;
using TravelWeb_API.Models.Itinerary.DTO;
namespace TravelWeb_API.Models.Itinerary.Service
{
    public class ItineraryService : IItineraryservice
    {
        private readonly TravelContext _context;
        private readonly ICloudinaryService _imageUploadService;

        public ItineraryService(TravelContext context, ICloudinaryService cloudinaryService)
        {
            _imageUploadService = cloudinaryService;
            _context = context;

        }
        /*建立主表包含物件*/
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
                    Introduction = dto.Introduction,
                    CurrentStatus = "Active" // 預設為啟用狀態
                };
                _context.Itineraries.Add(newItinerary);
                await _context.SaveChangesAsync(); // 先存一次拿 ID
                var initialVersion = new ItineraryVersion
                {
                    ItineraryId = newItinerary.ItineraryId,
                    VersionNumber = 1, // 或者叫 v1.0
                    Creator = 1,
                    CreateTime = DateTime.Now,
                    VersionRemark = "初始建立",
                    Source = "Manual", // 標記來源是手動建立
                    CurrentUsageStatus = "Y"    // 標記為當前使用版本
                };
                _context.ItineraryVersions.Add(initialVersion);
                await _context.SaveChangesAsync(); // 取得 VersionID
                // 2. 處理景點 (ItemsToPush)
                int currentOrder = 100;
                if (dto.ItemsToPush == null || !dto.ItemsToPush.Any())
                    throw new Exception("至少要一個景點");
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
                        DayNumber = 1, // 初始預設第一天
                        StartTime = dto.StartTime,
                        EndTime = dto.EndTime
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
        //抓行程細節
        public async Task<ItineraryDetailDto> GetItineraryDetailAsync(int itineraryId)
        {
            var result = await _context.Itineraries
                .Where(i => i.ItineraryId == itineraryId && i.CurrentStatus == "Active")
                .Select(i => new ItineraryDetailDto
                {
                    ItineraryId = i.ItineraryId,
                    ItineraryName = i.ItineraryName,
                    // 抓取「當前使用中」的版本
                    CurrentVersion = i.ItineraryVersions
                        .Where(v => v.CurrentUsageStatus == "Y")
                        .Select(v => new VersionDto
                        {
                            VersionId = v.VersionId,
                            VersionNumber = v.VersionNumber ?? 0,
                            // 抓取該版本下的所有項目，並依照 SortOrder 排序
                            Items = v.ItineraryItems
                                .OrderBy(item => item.SortOrder)
                                .Select(item => new ItemDetailDto
                                {
                                    ItemId = item.ItemId,
                                    SortOrder = item.SortOrder ?? 0,
                                    ContentDescription = item.ContentDescription,
                                    // 關鍵：從關聯的 Attraction 表抓取地點資訊
                                    AttractionName = item.Attraction.Name != null ? item.Attraction.Name : "未知景點",
                                    Address = item.Attraction.Address,
                                    Latitude = item.Attraction.Latitude,
                                    Longitude = item.Attraction.Longitude,
                                    StartTime = item.StartTime,
                                    EndTime = item.EndTime
                                }).ToList()
                        }).FirstOrDefault()
                }).FirstOrDefaultAsync();

            return result;
        }
        public async Task<int> SaveItinerarySnapshotAsync(ItinerarySnapshotDto dto)
        {
            string? imageUrl = null;
            if (dto.ImageFile != null)
            {
                imageUrl = await _imageUploadService.UploadImageAsync(dto.ImageFile, "itinerary_covers");
            }
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. 處理版本切換：將該行程的所有舊版本設為 "N" (不使用)
                var oldVersions = await _context.ItineraryVersions
                    .Where(v => v.ItineraryId == dto.ItineraryId && v.CurrentUsageStatus == "Y")
                    .ToListAsync();
                //上傳圖片
                var itinerary = await _context.Itineraries.FindAsync(dto.ItineraryId);
                if (itinerary == null) throw new Exception("找不到對應的行程主表"); // 拋出異常觸及 Catch 進行 Rollback
                if (imageUrl != null) itinerary.ItineraryImage = imageUrl;

                int nextVersionNumber = 1;
                if (oldVersions.Any())
                {
                    nextVersionNumber = oldVersions.Max(v => (v.VersionNumber ?? 0) + 1);
                    foreach (var v in oldVersions) v.CurrentUsageStatus = "N";
                }

                // 2. 建立新版本 (Snapshot)
                var newVersion = new ItineraryVersion
                {
                    ItineraryId = dto.ItineraryId,
                    VersionNumber = nextVersionNumber,
                    CreateTime = DateTime.Now,
                    Source = "Manual_Update",
                    VersionRemark = dto.VersionNote ?? $"版本 {nextVersionNumber}",
                    CurrentUsageStatus = "Y",
                    Creator = 1 // 實務上從登入資訊取得
                };
                _context.ItineraryVersions.Add(newVersion);
                await _context.SaveChangesAsync(); // 取得新 VersionId

                // 3. 重新計算 SortOrder 並寫入 Items
                // 按天數分組，每一天都從 100 開始重新編排
                foreach (var dayGroup in dto.Items.OrderBy(x => x.DayNumber).ThenBy(x => x.StartTime).GroupBy(x => x.DayNumber))
                {
                    int currentSortOrder = 100;
                    foreach (var item in dayGroup)
                    {
                        var newItem = new ItineraryItem
                        {
                            VersionId = newVersion.VersionId,
                            AttractionId = item.AttractionId,
                            DayNumber = dayGroup.Key,
                            SortOrder = currentSortOrder,
                            ContentDescription = item.ContentDescription,
                            StartTime = item.StartTime,
                            EndTime = item.EndTime
                        };
                        _context.ItineraryItems.Add(newItem);
                        currentSortOrder += 100; // 間隔 100，為拖拉留空間
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return newVersion.VersionId;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<bool> SoftDeleteItineraryAsync(int itineraryId)
        {
            var itinerary = await _context.Itineraries
        .FirstOrDefaultAsync(i => i.ItineraryId == itineraryId);

            // 2. 如果找不到，回傳 false
            if (itinerary == null) return false;

            // 3. 修改狀態 (假設你的非啟用狀態標記為 "N" 或 "Deleted")
            itinerary.CurrentStatus = "Inactive";

            // 4. 存檔並回傳結果
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<List<ItineraryVersionHistoryDto>> GetVersionHistoryAsync(int itineraryId)
        {
            return await _context.ItineraryVersions
                .Where(v => v.ItineraryId == itineraryId)
                .OrderByDescending(v => v.VersionNumber) // 最新版本排在最前面
                .Select(v => new ItineraryVersionHistoryDto
                {
                    VersionId = v.VersionId,
                    VersionNumber = v.VersionNumber ?? 0,
                    VersionRemark = v.VersionRemark,
                    CreateTime = (DateTime)v.CreateTime,
                    Source = v.Source,
                    IsCurrent = v.CurrentUsageStatus == "Y"
                })
                .ToListAsync();
        }
        public async Task<VersionDto> GetItemByVersionAsync(int versionId)
        {
#pragma warning disable CS8603 // 可能有 Null 參考傳回。
            return await _context.ItineraryVersions
        .Where(v => v.VersionId == versionId)
        .Select(v => new VersionDto
        {
            VersionId = v.VersionId,
            VersionNumber = v.VersionNumber ?? 0,
            VersionName = v.VersionRemark,
            // 抓取該版本底下的所有細項
            Items = v.ItineraryItems
                .OrderBy(item => item.DayNumber)
                .ThenBy(item => item.SortOrder)
                .Select(item => new ItemDetailDto
                {
                    ItemId = item.ItemId,
                    AttractionName = item.Attraction.Name,
                    DayNumber = item.DayNumber ?? 0,
                    SortOrder = item.SortOrder ?? 0,
                    StartTime = item.StartTime,
                    EndTime = item.EndTime,
                    ContentDescription = item.ContentDescription,
                    AttractionId = item.AttractionId ?? 0
                    // ... 其他你需要的欄位
                }).ToList()
        }).FirstOrDefaultAsync();
#pragma warning restore CS8603 // 可能有 Null 參考傳回。
        }
    }
}
