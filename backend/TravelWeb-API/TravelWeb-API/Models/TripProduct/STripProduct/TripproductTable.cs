using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models.TripProduct.ITripProduct;
using TravelWeb_API.Models.TripProduct.TripDTO;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TravelWeb_API.Models.TripProduct.STripProduct
{
    public class TripproductTable : ITripproductTable
    {
        private readonly TripDbContext _trip;
        public TripproductTable(TripDbContext trip)
        {
            _trip = trip;
        }
        //抓商品表的那張表給自己的DTO
        public async Task<IEnumerable<TripProductDTO>> GetAllAsync()
        {
            var products = await _trip.TripProducts.Select(p => new TripProductDTO
            {
                TripProductId = p.TripProductId,
                ProductName = p.ProductName,
                CoverImage = p.CoverImage,
                DisplayPrice = p.DisplayPrice
            }).ToListAsync();
            return products;
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


        public Task<IEnumerable<ProductQueryDTO>> GetTagAll()
        {
            throw new NotImplementedException();
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
                    CoverImage = x.p.CoverImage,
                    DisplayPrice = x.p.DisplayPrice,
                    RegionName = x.r.RegionName, // 這裡從 Join 的 r 拿名稱
                    DurationDays = x.p.DurationDays
                }).ToListAsync();

            return new PagedResult<TripProductDTO> { TotalCount = totalCount, Data = pagedData };

        }
    }
}
