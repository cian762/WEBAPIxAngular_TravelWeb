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
            _mvcBaseUrl = config["ExternalServices:MvcBackendUrl"]?.TrimEnd('/') ?? "";
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
        //刪除購物車的方法
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
                    dto.CoverImage = CartItemDTO.GetFullUrl(trip.CoverImage!, _mvcBaseUrl);
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
                        t.CoverImageUrl
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

        public Task RemoveItemAsync(int cartId)
        {
            throw new NotImplementedException();
        }
        //修改購物車內容
        public async Task UpdateQuantityAsync(UpdateCartQtyDTO dto)
        {
            // 1. 根據 CartId 找到購物車中的那一項
            var item = await _context.ShoppingCarts
                .FirstOrDefaultAsync(c => c.CartId == dto.CartId);

            if (item != null)
            {
                // 2. 直接更新數量 (前端傳多少，我們就改多少)
                item.Quantity = dto.Quantity;

                //選項：如果數量被改為 0 或以下，通常可以直接刪除該項目
                 if (dto.Quantity <= 0)
                {
                    _context.ShoppingCarts.Remove(item);
                }
                else
                {
                    _context.ShoppingCarts.Update(item);
                }

                _context.ShoppingCarts.Update(item);

                // 3. 存檔
                await _context.SaveChangesAsync();
            }
            else
            {
                // 找不到資料時，看你要報錯還是直接跳過
                throw new Exception("找不到該筆購物車項目");
            }
        }
        
     
    }
}
