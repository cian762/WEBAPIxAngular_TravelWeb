namespace TravelWeb_API.Models.TripProduct.TripDTO
{
    public class CartItemDTO
    {
        public int CartId { get; set; }        // 購物車流水號，刪除時會用到
        public string ?ProductCode { get; set; } // 產品代碼
        public string ?ProductName { get; set; } // 產品名稱 (從產品表抓)
        public decimal Price { get; set; }      // 單價 (從產品表抓)
        public int Quantity { get; set; }       // 購買數量
        public string ?CoverImage { get; set; }  // 產品圖片路徑 (記得拼上 MVC 網域)

        // 計算小計 (唯讀)
        public decimal SubTotal => Price * Quantity;
        // 這裡是原始從資料庫抓出來的路徑
        public string ?RawImage { get; set; }

        // 這是給前端看的最終網址
        public string ?FullImageUrl { get; set; }

        // 靜態方法：統一處理邏輯
        public static string GetFullUrl(string dbPath, string mvcBaseUrl)
        {
            if (string.IsNullOrEmpty(dbPath)) return "";

            // 1. 活動票：Cloudinary 雲端路徑 (包含 http)
            if (dbPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return dbPath;

            // 2. 商品票：去掉 wwwroot 
            var cleanPath = dbPath;
            if (cleanPath.StartsWith("wwwroot", StringComparison.OrdinalIgnoreCase))
            {
                cleanPath = cleanPath.Substring("wwwroot".Length);
            }

            // 3. 確保路徑格式正確
            cleanPath = cleanPath.Replace("~", "");
            if (!cleanPath.StartsWith("/")) cleanPath = "/" + cleanPath;

            return mvcBaseUrl.TrimEnd('/') + cleanPath;
        }
    }
    public class DeleteCartItemsDTO
    {
        public List<int> CartIds { get; set; } = new List<int>();
    }
    public class MigrateCartDto
    {
        public string GuestId { get; set; } = null!;
        public string MemberId { get; set; } = null!;
    }
    public class AddToCartDTO
    {
        public string? MemberId { get; set; }    // 哪個會員要買
        public string? ProductCode { get; set; } // 買哪個產品
        public int Quantity { get; set; }       // 買幾個
        public int? TicketCategoryId { get;set; }
    }
    public class UpdateCartQtyDTO
    {
        public string? MemberId { get; set; }
        public int CartId { get; set; }  // 指定哪一筆購物車資料
        public int Quantity { get; set; } // 新的數量
    }
}
