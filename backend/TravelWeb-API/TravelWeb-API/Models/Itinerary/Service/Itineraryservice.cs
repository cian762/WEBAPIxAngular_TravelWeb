using Microsoft.EntityFrameworkCore;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TravelWeb_API.Models.Itinerary.DBContext;
using TravelWeb_API.Models.Itinerary.DBModel;
using TravelWeb_API.Models.Itinerary.DTO;
namespace TravelWeb_API.Models.Itinerary.Service
{
    public class ItineraryService : IItineraryservice
    {
        private readonly TravelContext _context;
        private readonly ICloudinaryService _imageUploadService;
        private readonly IConfiguration _config;
        private readonly string _fontPath;
        static bool isfontexist = false;
        private static readonly object _fontLock = new object();
        private readonly GooglePlaceService _placesService;
        public ItineraryService(TravelContext context, ICloudinaryService cloudinaryService, IConfiguration config, GooglePlaceService placesService)
        {
            _imageUploadService = cloudinaryService;
            _context = context;
            _config = config;
            _fontPath = config["FileSettings:FontPath"];
            _placesService = placesService;
        }
        /*建立主表包含物件*/
        public async Task<int> CreateItineraryWithItemsAsync(ItineraryCreateDto dto, string memberid)
        {
            int totalDays = (dto.EndTime.Value.Date - dto.StartTime.Value.Date).Days + 1;
            // 啟動資料庫交易
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. 建立行程主表
                var newItinerary = new DBModel.Itinerary
                {
                    MemberId = memberid,
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

                if (dto.ItemsToPush == null || !dto.ItemsToPush.Any())
                    throw new Exception("至少要一個景點");
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
                        DayNumber = 1, // 初始預設第一天
                        StartTime = dto.StartTime,
                        EndTime = dto.StartTime?.AddHours(1)
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
                    ItineraryImage = i.ItineraryImage ?? "https://res.cloudinary.com/dcyrbbv4w/image/upload/v1773284561/cld-sample-2.jpg",
                    StartTime = i.StartTime,
                    EndTime = i.EndTime,
                    Introduction = i.Introduction,
                    // 抓取「當前使用中」的版本
                    CurrentVersion = i.ItineraryVersions
                        .Where(v => v.CurrentUsageStatus == "Y")
                        .Select(v => new VersionDto
                        {
                            VersionId = v.VersionId,
                            VersionNumber = v.VersionNumber ?? 0,
                            // 抓取該版本下的所有項目，並依照 SortOrder 排序
                            Items = v.ItineraryItems
                            .OrderBy(item => item.DayNumber) // 先按天數排
                                .ThenBy(item => item.SortOrder)
                                .Select(item => new ItemDetailDto
                                {
                                    AttractionId = item.AttractionId ?? 0,
                                    ItemId = item.ItemId,
                                    SortOrder = item.SortOrder ?? 0,
                                    DayNumber = item.DayNumber ?? 1,
                                    ContentDescription = item.ContentDescription,
                                    // 關鍵：從關聯的 Attraction 表抓取地點資訊
                                    AttractionName = item.AttractionName != null ? (item.Attraction.Name ?? item.ContentDescription) : item.ContentDescription,
                                    GooglePlaceId = item.Attraction.GooglePlaceId,
                                    PlaceId = item.Attraction.GooglePlaceId,
                                    PlaceName = item.Attraction.Name ?? null,
                                    Address = item.Attraction.Address != null ? item.Attraction.Address : "建議於附近區域安排",
                                    Latitude = item.Attraction.Latitude != null ? item.Attraction.Latitude : null,
                                    Longitude = item.Attraction.Longitude != null ? item.Attraction.Longitude : null,
                                    StartTime = item.StartTime,
                                    EndTime = item.EndTime
                                }).ToList()
                        }).FirstOrDefault()
                }).FirstOrDefaultAsync();

            return result;
        }
        //存取快照
        public async Task<int> SaveItinerarySnapshotAsync(ItinerarySnapshotDto dto)
        {
            //string? imageUrl = null;
            //if (dto.ImageFile != null)
            //{
            //    imageUrl = await _imageUploadService.UploadImageAsync(dto.ImageFile, "itinerary_covers");
            //}
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
                //if (imageUrl != null) itinerary.ItineraryImage = imageUrl;

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
                        int finalAttractionId = item.AttractionId;

                        // 如果前端傳過來的是新地點 (AttractionId == 0 或帶有 PlaceId)
                        if (finalAttractionId == 0 && !string.IsNullOrEmpty(item.PlaceId))
                        {
                            // 1. 檢查 DB 是否已有此 Google PlaceId 的景點
                            var existingAttraction = await _context.Attractions
                                .FirstOrDefaultAsync(a => a.GooglePlaceId == item.PlaceId);

                            if (existingAttraction != null)
                            {
                                finalAttractionId = existingAttraction.AttractionId;
                            }
                            else
                            {
                                // 2. 若不存在，則先新增到 Attractions 表
                                var newAttraction = new Attraction
                                {
                                    Name = item.Name,
                                    Address = item.Address,
                                    RegionId = 1000,
                                    GooglePlaceId = item.PlaceId,
                                    Latitude = item.Latitude,
                                    Longitude = item.Longitude,
                                    CreatedAt = DateTime.UtcNow
                                };
                                _context.Attractions.Add(newAttraction);
                                await _context.SaveChangesAsync(); // 取得新 AttractionId
                                finalAttractionId = newAttraction.AttractionId;
                            }
                        }
                        var newItem = new ItineraryItem
                        {
                            VersionId = newVersion.VersionId,
                            AttractionId = finalAttractionId,
                            DayNumber = dayGroup.Key,
                            SortOrder = currentSortOrder,
                            ContentDescription = item.Name,
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
        //軟刪除
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
        //查看過去版本
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
        //查看過去版本細項
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
        //儲存圖片
        public async Task<string> SaveImagebyid(IFormFile image, int Id)

        {
            // 1. 檢查檔案與行程是否存在
            if (image == null || image.Length == 0) return null;

            var itinerary = await _context.Itineraries.FindAsync(Id);
            if (itinerary == null) return null;

            // 2. 呼叫你的 Service 上傳到 Cloudinary
            // 這裡調用你提供的 UploadImageAsync
            string? imageUrl = await _imageUploadService.UploadImageAsync(image, "itinerary_covers");

            if (string.IsNullOrEmpty(imageUrl))
            {
                return null;
            }

            // 3. 同步更新資料庫中的圖片網址 (即時存入 DB)
            itinerary.ItineraryImage = imageUrl; // 假設欄位名為 ItineraryImage


            await _context.SaveChangesAsync();

            // 4. 回傳網址給前端，讓前端可以 [style.background-image] 顯示
            return imageUrl;
        }
        //額外增加一天
        public async Task<DateTime> ExtendOneDayAsync(int itineraryId)
        {
            var itinerary = await _context.Itineraries.FindAsync(itineraryId);
            if (itinerary == null) throw new Exception("找不到該行程");

            // 將結束日期往後推 1 天
            // 注意：如果是 Nullable 的 DateTime? 建議要做 null 檢查
            itinerary.EndTime = itinerary.EndTime?.AddDays(1) ?? DateTime.Now.AddDays(1);

            await _context.SaveChangesAsync();
            return itinerary.EndTime.Value;
        }
        #region GOOGLE地圖
        public async Task<DayItineraryDto> GetDayItineraryAsync(int itineraryId, int dayNumber)
        {
            var items = await _context.ItineraryItems
                .Include(x => x.Attraction)          // ← 必加
    .Include(x => x.Version) // ← Where 用到 Version 也要 Include
            .Where(x => x.Version.ItineraryId == itineraryId && x.DayNumber == dayNumber)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

            // AI 生成的地點可能沒有 PlaceId，這裡補齊
            foreach (var item in items.Where(x => x.Attraction != null && string.IsNullOrEmpty(x.Attraction.GooglePlaceId)))
            {
                try
                {
                    item.Attraction.GooglePlaceId = await _placesService.GetPlaceIdAsync(item.Attraction.Address);
                    Console.WriteLine($"AttractionId: {item.AttractionId}, Name: {item.Attraction?.Name}, PlaceId: {item.Attraction?.GooglePlaceId}");

                }
                catch (Exception ex)
                {
                    // 查不到就留空，不影響其他地點
                    Console.WriteLine($"Places API 查詢失敗：{item.Attraction.Address}, {ex.Message}");
                }
            }
            await _context.SaveChangesAsync();

            return new DayItineraryDto
            {
                DayNumber = dayNumber,
                Items = items.Where(x => x.Attraction != null).Select(x => new DayItineraryItemDto
                {
                    Order = x.SortOrder ?? 100,
                    PlaceId = x.Attraction.GooglePlaceId,
                    PlaceName = x.Attraction.Name,
                    Address = x.Attraction.Address,
                    ArrivalTime = x.StartTime.HasValue ? x.StartTime.Value.TimeOfDay : (TimeSpan?)null,
                    DepartureTime = x.EndTime.HasValue ? x.EndTime.Value.TimeOfDay : (TimeSpan?)null,
                }).ToList()
            };
        }

        #endregion
        #region  PDF
        //基於ID建成DTO給PDF
        public async Task<byte[]> GetExportFileAsync(int itineraryId)
        {
            var itinerary = await _context.Itineraries
          .Include(i => i.ItineraryVersions)
              .ThenInclude(v => v.ItineraryItems)
          .FirstOrDefaultAsync(i => i.ItineraryId == itineraryId);

            // 2. NULL 檢查：放在資料查詢之後
            if (itinerary == null) throw new Exception("找不到該行程");

            var activeVersion = itinerary.ItineraryVersions
                .FirstOrDefault(v => v.CurrentUsageStatus == "Y");

            if (activeVersion == null) throw new Exception("該行程沒有啟用的版本");

            // 3. 計算總天數 (基於行程起迄時間)
            int totalDays = 1;
            if (itinerary.StartTime.HasValue && itinerary.EndTime.HasValue)
            {
                totalDays = (itinerary.EndTime.Value.Date - itinerary.StartTime.Value.Date).Days + 1;
            }

            // 4. 組裝 DTO 並「補全天數」
            var exportDto = new ItineraryExportDto
            {
                ItineraryName = itinerary.ItineraryName,
                Introduction = itinerary.Introduction,
                StartTime = itinerary.StartTime,
                EndTime = itinerary.EndTime,
                Days = new List<ExportDayDetailDto>()
            };

            for (int d = 1; d <= totalDays; d++)
            {
                var dayDetail = new ExportDayDetailDto
                {
                    DayNumber = d,
                    // 從 activeVersion.ItineraryItems 中找尋該天的項目
                    Items = activeVersion.ItineraryItems
                        .Where(item => item.DayNumber == d)
                        .OrderBy(item => item.SortOrder)
                        .Select(item =>
                        {
                            var (parsedName, parsedDesc) = ParseAiContent(item.AttractionName, item.ContentDescription);
                            return new ExportItemDto
                            {
                                AttractionName = parsedName,       // 解析後的名稱
                                StartTime = item.StartTime?.ToString("HH:mm") ?? "未定",
                                ContentDescription = parsedDesc,   // 解析後的描述
                                Activity = item.ActivityId
                            };
                        }).ToList()
                };
                exportDto.Days.Add(dayDetail);
            }

            // 5. 確保字型已註冊並產生 PDF
            EnsureFontRegistered();
            return await GeneratePdfAsync(exportDto);
        }
        //輸出PDF
        public async Task<byte[]> GeneratePdfAsync(ItineraryExportDto data)
        {
            return await Task.Run(() =>
            {
                return Document.Create(container =>
             {
                 container.Page(page =>
                 {
                     page.Margin(50);
                     page.DefaultTextStyle(x => x.FontSize(12).FontFamily(_config["FileSettings:FontFamilyName"]));

                     // 標題區
                     page.Header().Text(data.ItineraryName).FontSize(24).SemiBold().FontColor(Colors.Blue.Medium);

                     page.Content().Column(col =>
                     {
                         col.Item().PaddingVertical(10).Text(data.Introduction);

                         // 依據行程天數生成內容
                         foreach (var day in data.Days)
                         {
                             col.Item().PaddingTop(10).Element(c => DayHeader(c, day.DayNumber));

                             if (day.Items != null && day.Items.Any())
                             {
                                 col.Item().Table(table =>
                                 {
                                     table.ColumnsDefinition(columns =>
                                     {
                                         columns.ConstantColumn(80); // 時間
                                         columns.RelativeColumn();   // 地點與活動
                                     });

                                     foreach (var item in day.Items)
                                     {
                                         table.Cell().Text(item.StartTime);
                                         table.Cell().Column(c =>
                                         {
                                             c.Item().Text(item.AttractionName).Bold();
                                             c.Item().Text(item.ContentDescription).FontSize(10).FontColor(Colors.Grey.Medium);
                                         });
                                     }
                                 });
                             }
                             else
                             {
                                 col.Item().Text(t => { t.Span("今日暫無排定行程").Italic().FontColor(Colors.Grey.Lighten1); });
                             }
                         }
                     });
                 });
             }).GeneratePdf();
            });
        }
        //輔助PDF美化標題的方法
        private void DayHeader(IContainer container, int dayNum)
        {
            container.Background(Colors.Grey.Lighten3).Padding(5).Text($"第 {dayNum} 天").SemiBold();
        }
        //判斷字形是否載入的方法
        private void EnsureFontRegistered()
        {
            if (!isfontexist)
            {
                lock (_fontLock)
                {
                    if (!isfontexist) // Double-check locking
                    {
                        if (File.Exists(_fontPath))
                        {
                            // 建議使用 File.OpenRead 讀取後要 Dispose，或者直接傳路徑字串
                            FontManager.RegisterFont(File.OpenRead(_fontPath));
                            isfontexist = true;
                        }
                    }
                }
            }
        }
        //用以修改AI生成結果變為可被理解的字串
        private (string name, string description) ParseAiContent(string? attractionName, string? contentDescription)
        {
            var desc = contentDescription ?? "";

            if (desc.StartsWith("[AI_NEW_PLACE]"))
            {
                // 格式: [AI_NEW_PLACE]|名稱|PlaceId|地址|緯度|經度|細節
                var parts = desc.Split('|');
                var name = (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
                    ? parts[1]
                    : "未知地點";
                var detail = parts.Length > 6 ? parts[6] : "";
                return (name, detail);
            }

            // 非 AI 生成，直接回傳原本的值
            return (attractionName ?? "未知地點", desc);
        }
        #endregion
    }
}
