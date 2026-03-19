using TravelWeb_API.Models.TripProduct.TripDTO;

namespace TravelWeb_API.Models.TripProduct.ITripProduct
{
    public interface ITripproductTable
    {

        // 2. 核心功能：根據前端傳來的 DTO 條件進行搜尋
        // 傳入搜尋條件 (QueryDTO)，回傳產品清單 (ProductDTO)
        Task<PagedResult<TripProductDTO>> SearchProductsAsync(ProductQueryDTO queryDto);

        // 3. 取得所有標籤 (供前端渲染上方那排標籤按鈕)
        // 注意：這裡回傳的應該是 Tag 的基本資訊，不是 QueryDTO
        Task<IEnumerable<TagListDTO>> GetTagsAllAsync();

        // 4. 取得所有地區 (供前端渲染地區切換)
        Task<IEnumerable<RegionListDTO>> GetRegionsAllAsync();
    }

    // 額外定義簡單的 DTO 給選單使用
    public class TagListDTO { public int TagId { get; set; } public string ?TagName { get; set; } }
    public class RegionListDTO { public int RegionId { get; set; } public string ?RegionName { get; set; } }
    public class PagedResult<T>
    {
        public int TotalCount { get; set; } // 搜尋結果的總筆數
        public IEnumerable<T> ?Data { get; set; } // 當前頁面的資料內容
    }
    //=============================================================================================
    //這裡是商品詳細頁



}
