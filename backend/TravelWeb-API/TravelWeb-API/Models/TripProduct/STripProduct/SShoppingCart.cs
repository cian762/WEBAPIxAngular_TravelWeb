using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models.TripProduct.ITripProduct;
using TravelWeb_API.Models.TripProduct.TripDTO;

namespace TravelWeb_API.Models.TripProduct.STripProduct
{
    public class SShoppingCart: IShoppingCart
    {
     private readonly TripDbContext _context;

     private readonly string _mvcBaseUrl;

        public SShoppingCart(TripDbContext context,IConfiguration config)
        {
            _context = context;
            _mvcBaseUrl = config["AppSettings:MvcDomain"]?.TrimEnd('/') ?? "";
        }
        //加入購物車的方法
        public async Task AddToCartAsync(AddToCartDTO dto)
        {
            // 檢查 Unique Index (MemberId + ProductCode)
            var existingItem = await _context.ShoppingCarts
                .FirstOrDefaultAsync(c => c.MemberId == dto.MemberId && c.ProductCode == dto.ProductCode);

            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
                _context.ShoppingCarts.Update(existingItem);
            }
            else
            {
                var newItem = new ShoppingCart
                {
                    MemberId = dto.MemberId!,
                    ProductCode = dto.ProductCode,
                    Quantity = dto.Quantity,
                    TicketCategoryId = dto.TicketCategoryId, // 前端傳入
                    CreatedAt = DateTime.Now
                };
                await _context.ShoppingCarts.AddAsync(newItem);
            }
            await _context.SaveChangesAsync();
        }
        //使用者按下「確定購買」或完成結單後。
        public async Task ClearCartAsync(string memberId)
        {
           var clear = await _context.ShoppingCarts.Where(u=>u.MemberId==memberId).ToListAsync();
            if (clear.Any()) 
            {
                _context.ShoppingCarts.RemoveRange(clear);
                await _context.SaveChangesAsync();
            }
        }
        //由會員ID抓出該會員底下所有商品
        public async Task<IEnumerable<CartItemDTO>> GetCartAsync(string memberId)
        {
            // 1. 先抓出基礎購物車資料
            var cartItems = await _context.ShoppingCarts
                .Where(c => c.MemberId == memberId)
                .ToListAsync();

            var resultList = new List<CartItemDTO>();

            foreach (var item in cartItems)
            {
                var dto = new CartItemDTO
                {
                    CartId = item.CartId,
                    ProductCode = item.ProductCode,
                    Quantity = item.Quantity ?? 0,
                };

                // --- 第一段：找「行程商品」(重點：s.Price 是檔期價格) ---
                // 1. 行程票 (合併日期到名字)
                var trip = await _context.TripSchedules
                    .Where(s => s.ProductCode == item.ProductCode)
                    .Select(s => new {
                        s.TripProduct.ProductName,
                        s.TripProduct.CoverImage,
                        s.Price,
                        s.StartDate,
                        s.EndDate
                    }).FirstOrDefaultAsync();

                if (trip != null)
                {
                    dto.ProductName = $"{trip.ProductName} ({trip.StartDate:yyyy/MM/dd} ~ {trip.EndDate:MM/dd})";
                    dto.Price = trip.Price ?? 0;
                    string url ="/PImages" + trip.CoverImage!;
                    dto.CoverImage = CartItemDTO.GetFullUrl(url,_mvcBaseUrl);
                    resultList.Add(dto);
                    continue;
                }

                // --- 第二段：找「景點票券」(假設景點圖在 AttractionProducts 裡) ---
                var attr = await _context.AttractionProducts
                .Where(a => a.ProductCode == item.ProductCode)
                .Select(a => new
                {
                    a.Attraction.Name,
                    a.Title,
                    a.Price,
                    // 透過 Attraction 導覽屬性去抓 Images 集合裡的第一張圖
                    CoverImage = a.Attraction.Images.Select(img => img.ImagePath).FirstOrDefault()
                })
                  .FirstOrDefaultAsync();

                if (attr != null)
                {
                    dto.ProductName = $"{attr.Name} ({attr.Title})";
                    dto.Price = attr.Price ?? 0;
                    dto.CoverImage = CartItemDTO.GetFullUrl(attr.CoverImage!, _mvcBaseUrl);

   
                    resultList.Add(dto);
                    continue;
                }

                // --- 第三段：找「活動票券」(ImageUrl 存的是 Cloudinary 網址) ---
                var act = await _context.AcitivityTickets
                    .Where(t => t.ProductCode == item.ProductCode)
                    .Select(t => new {
                        t.ProductName,
                        t.CoverImageUrl,
                        t.CurrentPrice
                        // 直接拿這張表的圖片欄位！
                    })
                    .FirstOrDefaultAsync();

                if (act != null)
                {
                    dto.ProductName = act.ProductName;
                    dto.Price = 0;

                    // 直接把這張表的 ActivityImageUrl 丟進去處理
                    dto.CoverImage = CartItemDTO.GetFullUrl(act.CoverImageUrl!, _mvcBaseUrl);

                    resultList.Add(dto);
                }
            }
            return resultList;
        }
        //遊客跟使用者搬遷購物車內容
        public async Task MigrateCartAsync(string guestId, string memberId)
        {
            // 1. 安全檢查：如果 ID 一樣，直接結束
            if (guestId == memberId) return;

            // 2. 身分判定：利用我們約定的 "GUEST_" 前綴
            if (!guestId.StartsWith("GUEST_"))
            {
                throw new Exception("無效的遊客識別碼");
            }

            // 3. 確保目標 memberId 真的存在於會員資料表 (避免把東西搬到不存在的帳號)
            var isRealMember = await _context.MemberInformations
                .AnyAsync(m => m.MemberId == memberId);

            if (!isRealMember)
            {
                throw new Exception("目標會員帳號不存在");
            }

            // 4. 抓取遊客的所有購物車項目
            var guestItems = await _context.ShoppingCarts
                .Where(c => c.MemberId == guestId)
                .ToListAsync();

            if (!guestItems.Any()) return;

            // 5. 抓取會員原本的購物車項目 (用來比對重複)
            var memberItems = await _context.ShoppingCarts
                .Where(c => c.MemberId == memberId)
                .ToListAsync();

            foreach (var guestItem in guestItems)
            {
                // 檢查會員是否買過同樣的產品 (ProductCode)
                var duplicateItem = memberItems
                    .FirstOrDefault(m => m.ProductCode == guestItem.ProductCode);

                if (duplicateItem != null)
                {
                    // 如果重複，把遊客的數量加給會員，然後刪除遊客這筆
                    duplicateItem.Quantity += guestItem.Quantity;
                    _context.ShoppingCarts.Remove(guestItem);
                }
                else
                {
                    // 如果沒重複，直接把這筆遊客紀錄「過戶」給會員
                    guestItem.MemberId = memberId;
                }
            }

            // 6. 存檔
            await _context.SaveChangesAsync();
        }

        //刪除購物車支援多選跟單選
        public async Task RemoveItemsAsync(List<int> cartIds, string memberId)
        {
            // 1. 找出所有在 ID 清單內，且屬於該會員的購物車項目
            var itemsToDelete = await _context.ShoppingCarts
                .Where(c => cartIds.Contains(c.CartId) && c.MemberId == memberId)
                .ToListAsync();

            // 2. 如果有找到資料，執行批次刪除
            if (itemsToDelete.Any())
            {
                _context.ShoppingCarts.RemoveRange(itemsToDelete);
                await _context.SaveChangesAsync();
            }
        }
        //修改購物車內容
        public async Task UpdateQuantityAsync(UpdateCartQtyDTO dto,string memberId)
        {
            // 同時比對 CartId 與 MemberId
            var item = await _context.ShoppingCarts
                .FirstOrDefaultAsync(c => c.CartId == dto.CartId && c.MemberId == memberId);

            if (item != null)
            {
                if (dto.Quantity <= 0)
                {
                    _context.ShoppingCarts.Remove(item);
                }
                else
                {
                    item.Quantity = dto.Quantity;
                    _context.ShoppingCarts.Update(item);
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new Exception("找不到該筆購物車項目或您無權限修改");
            }
        }
        
     
    }
}
