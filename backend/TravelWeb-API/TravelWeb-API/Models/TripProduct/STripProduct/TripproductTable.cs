using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models.TripProduct.ITripProduct;
using TravelWeb_API.Models.TripProduct.TripDTO;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TravelWeb_API.Models.TripProduct.STripProduct
{
    public class TripproductTable : ITripproductTable
    {
        private readonly TripDbContext _trip;
        private readonly string _mvcBaseUrl;
        private readonly string _mvchung;
        public TripproductTable(TripDbContext trip, IConfiguration config, IConfiguration config2)
        {
            _trip = trip;
            _mvcBaseUrl = config["AppSettings:MvcDomain"]?.TrimEnd('/') ?? "";
            _mvchung = config2["AppSettings:Mvchung"]?? "";

        }

        //抓所有地區表
        public async Task<IEnumerable<RegionListDTO>> GetRegionsAllAsync()
        {
            var rogin = await _trip.TripRegions.Select(r => new RegionListDTO
            {
                RegionId = r.RegionId,
                RegionName = r.RegionName
            }).ToListAsync();
            return rogin;
        }

        // 取得所有標籤，給前端畫按鈕用
        public async Task<IEnumerable<TagListDTO>> GetTagsAllAsync()
        {
            var tags = await _trip.TravelTags.Select(s => new TagListDTO
            {
                TagId = s.TravelTagid,
                TagName = s.TravelTagName
            }).ToListAsync();
            return tags;
        }
        //搜尋條件跟分頁
        public async Task<PagedResult<TripProductDTO>> SearchProductsAsync(ProductQueryDTO queryDto)
        {
            // 1. 先從產品表開始 (IQueryable)
            var query = _trip.TripProducts.AsQueryable();

            // 2. 篩選：關鍵字、地區ID、標籤、價格 (不需要 Join 就能做的先做)
            // 1. 篩選地區
            if (queryDto.RegionId.HasValue)
                query = query.Where(p => p.RegionId == queryDto.RegionId);
            //價錢篩選
            if (queryDto.MaxPrice.HasValue)
                query = query.Where(p => p.DisplayPrice <= queryDto.MaxPrice);

            // 2. 篩選標籤 (多對多直接寫法)
            if (queryDto.TagIds != null && queryDto.TagIds.Any())
            {
                query = query.Where(p => p.TravelTags.Any(t => queryDto.TagIds.Contains(t.TravelTagid)));
            }

            // 3. 篩選關鍵字與價格
            if (!string.IsNullOrEmpty(queryDto.Keyword))
                query = query.Where(p => p.ProductName!.Contains(queryDto.Keyword));

            if (queryDto.MinPrice.HasValue)
                query = query.Where(p => p.DisplayPrice >= queryDto.MinPrice);

            // --- 計算總筆數 (分頁前) ---
            var totalCount = await query.CountAsync();

            // 3. 【重點】手動 Join 地區表，為了拿 RegionName
            var queryWithRegion = query.Join(
                _trip.TripRegions,         // 要 Join 的表
                p => p.RegionId,           // 產品表的 Key
                r => r.RegionId,           // 地區表的 Key
                (p, r) => new { p, r }     // 組合結果
            );

            // 4. 排序
            queryWithRegion = queryDto.SortType switch
            {
                1 => queryWithRegion.OrderBy(x => x.p.DisplayPrice),
                _ => queryWithRegion.OrderByDescending(x => x.p.TripProductId)
            };

            // 5. 分頁與投影
            var pagedData = await queryWithRegion
                .Skip((queryDto.Page - 1) * queryDto.PageSize)
                .Take(queryDto.PageSize)
                .Select(x => new TripProductDTO
                {
                    TripProductId = x.p.TripProductId,
                    ProductName = x.p.ProductName,
                    CoverImage = _mvchung+x.p.CoverImage,
                    DisplayPrice = x.p.DisplayPrice,
                    RegionName = x.r.RegionName, // 這裡從 Join 的 r 拿名稱
                    DurationDays = x.p.DurationDays
                }).ToListAsync();

            return new PagedResult<TripProductDTO> { TotalCount = totalCount, Data = pagedData };

        }
        // 取得熱門行程 (依點擊次數排序，取前 8 筆)
        public async Task<IEnumerable<TripProductDTO>> GetHotProductsAsync(int take = 8)
        {
            return await _trip.TripProducts
                .OrderByDescending(p => p.ClickTimes) // 依熱門程度排序
                .Take(take)
                .Select(p => new TripProductDTO
                {
                    TripProductId = p.TripProductId,
                    ProductName = p.ProductName,
                    CoverImage = _mvchung + p.CoverImage,
                    DisplayPrice = p.DisplayPrice,
                    RegionName = p.Region != null ? p.Region.RegionName : "",
                    DurationDays = p.DurationDays,
                    // 如果需要標籤，記得在這裡 Select
                    CategoryTags = p.TravelTags.Select(t => t.TravelTagName!).ToList()
                })
                .ToListAsync();
        }
        //=====================================================================================
        //這裡是商品詳細頁
        //細項標頭
        public async Task<ProductBasicDto?> GetBasicInfoAsync(int id)
        {
            var product = await _trip.TripProducts.Where(x => x.TripProductId == id).Select(s => new ProductBasicDto
            {
                TripProductId = s.TripProductId,
                ProductName = s.ProductName!,
                CoverImage = _mvchung + s.CoverImage,
                Description = s.Description,
                RegionName = s.Region != null ? s.Region.RegionName : "未分類",
                Tags = s.TravelTags!
                      .Select(t => t.TravelTagName!).ToList()
            }).FirstOrDefaultAsync();
            var viewCount = await _trip.TripProducts.FirstOrDefaultAsync(x => x.TripProductId == id);
            if (viewCount != null) { viewCount.ClickTimes += 1;await _trip.SaveChangesAsync(); }
            
            return product;
        }
        //細項行程
        public async Task<IEnumerable<ProductItineraryDto>> GetItineraryAsync(int id)
        {
            return await _trip.TripItineraryItems
         .Where(i => i.TripProductId == id)
         // 先按天數排序，再按當天順序排序
         .OrderBy(i => i.DayNumber)
         .ThenBy(i => i.SortOrder)
         .Select(i => new ProductItineraryDto
         {
             DayNumber = (int)i.DayNumber!,
             SortOrder =(int)i.SortOrder!,
             CustomText = i.CustomText,
             DefaultDescription=i.Resource!.DefaultDescription,
             // 從 Resource 表抓名字 (例如：台北 101)
             ResourceName = i.Resource != null ? i.Resource.ResourceName : null,

             // 核心邏輯：去 ResourcesImage 表抓出所有屬於該 ResourceId 的圖片
             ResourceUrls = i.Resource != null
                ? i.Resource.ResourcesImages
                    .Select(img => _mvcBaseUrl + img.MainImage)
                    .ToList()
                : new List<string>()
         })
         .ToListAsync();
        }
        //細項檔期
        public async Task<IEnumerable<ProductScheduleDto>> GetSchedulesAsync(int id)
        {
            // 1. 將今天的日期設定為 DateOnly 型別，以便與資料庫欄位對比
            // 使用 DateOnly.FromDateTime 從 DateTime 轉換過來
            var today = DateOnly.FromDateTime(DateTime.Today);

            var result = await _trip.TripSchedules
                .Where(s => s.TripProductId == id && s.StartDate >= today) // 現在兩邊都是 DateOnly 了
                .OrderBy(s => s.StartDate)
                .ToListAsync();

            // 2. 在記憶體中進行轉型
            return result.Select(s => new ProductScheduleDto
            {
                ProductCode = s.ProductCode,
                // 如果 s.StartDate 已經是 DateOnly，直接賦值即可
                // 若為可空型別且 Dto 需要非空，需處理預設值，例如：s.StartDate ?? DateOnly.MinValue
                StartDate = s.StartDate ?? DateOnly.MinValue,
                Price = (decimal)s.Price!,
                AvailableStock = (s.MaxCapacity ?? 0) - (s.SoldQuantity ?? 0)
            });

        }
    }
}

