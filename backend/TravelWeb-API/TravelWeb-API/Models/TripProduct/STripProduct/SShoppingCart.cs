using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models.ActivityModel;
using TravelWeb_API.Models.TripProduct.ITripProduct;
using TravelWeb_API.Models.TripProduct.TripDTO;

namespace TravelWeb_API.Models.TripProduct.STripProduct
{
    public class SShoppingCart: IShoppingCart
    {
     private readonly TripDbContext _context;

     private readonly ActivityDbContext _acts;

     private readonly string _mvcBaseUrl;
     private readonly string _mvcurlhung;

        public SShoppingCart(TripDbContext context,IConfiguration config, ActivityDbContext acts, IConfiguration config2)
        {
            _context = context;
             _acts = acts;
            _mvcBaseUrl = config["AppSettings:MvcDomain"]?.TrimEnd('/') ?? "";
            _mvcurlhung = config2["AppSettings:Mvchung"]?? "";
            
        }
        //加入購物車的方法
        public async Task AddToCartAsync(AddToCartDTO dto)
        {
            // 💡 關鍵修正：判斷條件必須包含 TicketCategoryId
            var existingItem = await _context.ShoppingCarts
                .FirstOrDefaultAsync(c =>
                    c.MemberId == dto.MemberId &&
                    c.ProductCode == dto.ProductCode &&
                    c.TicketCategoryId == dto.TicketCategoryId); // ✨ 加上這一行

            if (existingItem != null)
            {
                // 如果「同產品且同票種」已存在，才累加數量
                existingItem.Quantity += dto.Quantity;
                _context.ShoppingCarts.Update(existingItem);
            }
            else
            {
                // 如果票種不同，視為新項目，執行 Add
                var newItem = new ShoppingCart
                {
                    MemberId = dto.MemberId!,
                    ProductCode = dto.ProductCode,
                    Quantity = dto.Quantity,
                    TicketCategoryId = dto.TicketCategoryId,
                    CreatedAt = DateTime.Now
                };
                await _context.ShoppingCarts.AddAsync(newItem);
            }
            await _context.SaveChangesAsync();
        }
        //使用者按下「確定購買」或完成結單後清空。
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
                    ticketCategoryId = item.TicketCategoryId
                };

                // --- 第一段：找「行程商品」(重點：s.Price 是檔期價格) ---
                // 1. 行程票 (合併日期到名字)
                var trip = await _context.TripSchedules
                    .Where(s => s.ProductCode == item.ProductCode)
                    .Select(s => new {
                        s.TripProductId,
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
                    string url =trip.CoverImage!;
                    dto.CoverImage = CartItemDTO.GetFullUrl(url,_mvcurlhung);
                    dto.TargetId = trip.TripProductId;
                    resultList.Add(dto);
                    continue;
                }

                // --- 第二段：找「景點票券」(假設景點圖在 AttractionProducts 裡) ---
                var attr = await _context.AttractionProducts
                .Where(a => a.ProductCode == item.ProductCode)
                .Select(a => new
                {
                    a.AttractionId,
                    a.Attraction.Name,
                    a.Title,
                    a.Price,
                    // 透過 Attraction 導覽屬性去抓 Images 集合裡的第一張圖
                    CoverImage = a.Attraction.Images.Select(img => img.ImagePath).FirstOrDefault()
                })
                  .FirstOrDefaultAsync();

                if (attr != null)
                {
                    dto.TargetId = attr.AttractionId;
                    dto.ProductName = $"{attr.Name} ({attr.Title})";
                    dto.Price = attr.Price ?? 0;
                    dto.CoverImage = CartItemDTO.GetFullUrl(attr.CoverImage!, _mvcBaseUrl);

   
                    resultList.Add(dto);
                    continue;
                }

                // --- 第三段：找「活動票券」(ImageUrl 存的是 Cloudinary 網址) ---
                // --- 第三段：找「活動票券」(ACT) ---
                // 1. 先從 _context (TripDbContext) 抓出票券基本資訊
                var ticket = await _context.AcitivityTickets
                    .Where(t => t.ProductCode == item.ProductCode)
                    .Select(t => new {
                        t.ProductCode,
                        t.ProductName,
                        t.CoverImageUrl,
                        t.CurrentPrice
                    }).FirstOrDefaultAsync();

                if (ticket != null)
                {
                    // 2. 使用拿到 ProductCode，去 _act (ActivityDbContext) 的中間表查 ActivityID
                    // 因為這張表在另一個 DB，所以要分開查詢
                    var detail = await _acts.ActivityTicketDetails
                        .Where(d => d.ProductCode == ticket.ProductCode)
                        .Select(d => new { d.ActivityId })
                        .FirstOrDefaultAsync();

                    if (detail != null)
                    {
                        dto.TargetId = (int)detail.ActivityId!; // 💡 成功從另一個 DB 抓到導頁 ID
                        dto.ProductName = ticket.ProductName;
                        dto.Price = (decimal)(ticket.CurrentPrice ?? 0);
                        dto.CoverImage = ticket.CoverImageUrl;
                        dto.TargetId = (int)(detail.ActivityId ?? 0);

                        resultList.Add(dto);
                    }
                }

            }
            return resultList;
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
        //處理「登入後將遊客本地（LocalStorage）的大量資料同步到資料庫」處理遊客搬遷到會員
        public async Task SyncCartAsync(List<AddToCartDTO> dtos, string memberId)
        {
            // 1. 一次抓出該會員目前資料庫裡所有的購物車項目
            var existingCart = await _context.ShoppingCarts
                .Where(c => c.MemberId == memberId)
                .ToListAsync();

            foreach (var dto in dtos)
            {
                // 2. 在記憶體中比對是否已存在
                var existingItem = existingCart
                    .FirstOrDefault(c => c.ProductCode == dto.ProductCode && c.TicketCategoryId == dto.TicketCategoryId);

                if (existingItem != null)
                {
                    // 如果已存在，累加數量（或是你可以改成覆蓋）
                    existingItem.Quantity += dto.Quantity;
                }
                else
                {
                    // 如果不存在，新增一筆
                    _context.ShoppingCarts.Add(new ShoppingCart
                    {
                        MemberId = memberId,
                        ProductCode = dto.ProductCode,
                        Quantity = dto.Quantity,
                        TicketCategoryId = dto.TicketCategoryId,
                        CreatedAt = DateTime.Now
                    });
                }
            }

            // 3. 最終只呼叫一次資料庫存檔，效率極高！
            await _context.SaveChangesAsync();
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
