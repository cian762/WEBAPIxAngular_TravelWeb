using Microsoft.EntityFrameworkCore;
using Stateless;
using TravelWeb_API.Models.TripProduct.Enums;
using TravelWeb_API.Models.TripProduct.ITripProduct;
using TravelWeb_API.Models.TripProduct.TripDTO;

namespace TravelWeb_API.Models.TripProduct.STripProduct
{
    public class SOrder : IOrder
    {
        private readonly TripDbContext _context;
        private readonly string _mvcBaseUrl;

        public SOrder(TripDbContext context, IConfiguration config)
        {
            _context = context;
            _mvcBaseUrl = config["AppSettings:MvcDomain"]?.TrimEnd('/') ?? "";
        }
        // 5. 取消訂單：僅限未付款且 Pending 狀態，更新狀態為 Cancelled
        public async Task<bool> CancelOrderAsync(int orderId, string memberId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null || order.MemberId != memberId) { return false; }
            // 2. 初始化狀態機
            // 我們告訴狀態機：目前的狀態在 order.OrderStatus，變更時請寫回該欄位
            var machine = new StateMachine<string, OrderTrigger>(() => order.OrderStatus!, s => order.OrderStatus = s);
            // 3. 配置狀態機規則（這就是你的業務邏輯）
            // 只有在 Pending (待處理) 時，才准許執行 UserCancel
            machine.Configure(OrderStatusNames.Pending)
                .Permit(OrderTrigger.UserCancel, OrderStatusNames.Cancelled);
            // 4. 檢查是否可以執行「取消」動作
            if (machine.CanFire(OrderTrigger.UserCancel))
            {
                // 執行動作（這會自動把 order.OrderStatus 改成 "Cancelled"）
                machine.Fire(OrderTrigger.UserCancel);

                // 5. 存檔並回傳成功
                await _context.SaveChangesAsync();
                return true;
            }

            // 如果目前的狀態（例如：已經是 Completed 或 Active）不能取消，則回傳失敗
            return false;
        }
        // 1. 建立訂單：處理 4 張表寫入與 Transaction，回傳產生的 OrderId
        public async Task<Order> CreateOrderAsync(CreateOrderDto dto, string memberId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. 決定商品清單來源 (不變)
                var itemsToProcess = dto.DirectBuyItems;
                if (itemsToProcess == null || !itemsToProcess.Any())
                {
                    itemsToProcess = await _context.ShoppingCarts
                        .Where(c => c.MemberId == memberId)
                        .Select(c => new AddToCartDTO
                        {
                            ProductCode = c.ProductCode,
                            Quantity = c.Quantity ?? 2,
                            TicketCategoryId = c.TicketCategoryId ?? 0
                        }).ToListAsync();
                }

                if (!itemsToProcess.Any()) throw new Exception("結帳清單不可為空");

                // 2. 算錢 (不變)
                decimal totalAmount = await InternalCalculateTotalAsync(itemsToProcess);

                // 3. 建立訂單主檔 (暫時不要 SaveChanges)
                var order = new Order
                {
                    MemberId = memberId,
                    TotalAmount = totalAmount,
                    ContactName = dto.ContactName,
                    ContactPhone = dto.ContactPhone,
                    ContactEmail = dto.ContactEmail,
                    CustomerNote = dto.CustomerNote,
                    CreatedAt = DateTime.Now,
                    OrderStatus = OrderStatusNames.Pending,
                    PaymentStatus = "Unpaid"
                };
                _context.Orders.Add(order);

                // 4. 處理訂單明細
                foreach (var item in itemsToProcess)
                {
                    var productName = await GetProductNameSnapshotAsync(item.ProductCode!);
                    DateOnly? startDate = null;
                    DateOnly? endDate = null;

                    var tripSchedule = await _context.TripSchedules
                        .FirstOrDefaultAsync(ts => ts.ProductCode == item.ProductCode);

                    if (tripSchedule != null)
                    {
                        if (tripSchedule.MaxCapacity.HasValue)
                        {
                            int currentSold = tripSchedule.SoldQuantity ?? 0;
                            int remaining = tripSchedule.MaxCapacity.Value - currentSold;
                            if (remaining < item.Quantity) throw new Exception($"行程 [{item.ProductCode}] 剩餘名額不足！");
                            tripSchedule.SoldQuantity = currentSold + item.Quantity;
                        }
                        startDate = tripSchedule.StartDate;
                        endDate = tripSchedule.EndDate;
                    }
                    else
                    {
                        var stockRecord = await _context.StockInRecords.FirstOrDefaultAsync(s => s.ProductCode == item.ProductCode);
                        if (stockRecord != null)
                        {
                            if (stockRecord.RemainingStock < item.Quantity) throw new Exception($"商品庫存不足");
                            stockRecord.RemainingStock -= item.Quantity;
                        }
                    }

                    // 💡 關鍵點 A：改用物件關聯
                    var orderItem = new OrderItem
                    {
                        Order = order, // ✅ 這裡直接給物件，不要給 OrderId
                        ProductCode = item.ProductCode,
                        ProductNameSnapshot = productName,
                        StartDateSnapshot = startDate,
                        EndDateSnapshot = endDate
                    };
                    _context.OrderItems.Add(orderItem);
                    // ❌ 刪除這裡的 SaveChangesAsync()

                    var ticketCat = await _context.TicketCategories.FirstOrDefaultAsync(tc => tc.TicketCategoryId == item.TicketCategoryId);
                    var unitPrice = await GetSpecificUnitPriceAsync(item.ProductCode!, (int)item.TicketCategoryId!);

                    // 💡 關鍵點 B：改用物件關聯
                    _context.OrderItemTickets.Add(new OrderItemTicket
                    {
                        OrderItem = orderItem, // ✅ 這裡直接給物件，EF 會自動處理 OrderItemId
                        TicketCategoryId = (int)item.TicketCategoryId!,
                        TicketNameSnapshot = ticketCat?.CategoryName ?? "一般票",
                        UnitPrice = unitPrice,
                        Quantity = item.Quantity
                    });
                }

                // 5. 建立初始交易紀錄
                var paymentTransaction = new PaymentTransaction
                {
                    Order = order, // ✅ 一樣給物件
                    PaymentProvider = "綠界科技",
                    PaidAmount = order.TotalAmount,
                    TransactionStatus = "待處理",
                    CreatedAt = DateTime.Now
                };
                _context.PaymentTransactions.Add(paymentTransaction);

                // 💡 最終魔幻時刻：一次存檔，一次 Commit
                // EF Core 會自動幫你排順序：先存 Order 拿 ID -> 填入 OrderItem -> 拿 ID -> 填入 Ticket
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return order;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // 挖出真正底層的錯誤 (例如：欄位太長、外鍵失敗)
                var detailedMsg = ex.InnerException?.InnerException?.Message ?? ex.InnerException?.Message ?? ex.Message;
                throw new Exception($"訂單建立失敗: {detailedMsg}");
            }
        }
        // 2. 訂單預覽：在正式下單前，計算金額與確認商品清單 (不寫入資料庫)
        public async Task<OrderDetailDto> GetCheckoutPreviewAsync(CreateOrderDto dto, string memberId)
        {
            // 1.決定來源(直接購買 或 購物車)
            List<AddToCartDTO> itemsToProcess;

            if (dto.DirectBuyItems != null && dto.DirectBuyItems.Any())
            {
                // 優先使用直接購買的商品
                itemsToProcess = dto.DirectBuyItems;
            }
            else
            {
                itemsToProcess = await _context.ShoppingCarts
                    .Where(c => c.MemberId == memberId)
                    .Select(c => new AddToCartDTO
                    {
                        ProductCode = c.ProductCode,
                        Quantity = c.Quantity ?? 1,
                        TicketCategoryId = c.TicketCategoryId ?? 1
                    }).ToListAsync();
            }

            if (!itemsToProcess.Any()) throw new Exception("結帳清單不可為空");

            // 2. 計算總金額 (不寫入，純試算)
            decimal totalAmount = await InternalCalculateTotalAsync(itemsToProcess);

            var preview = new OrderDetailDto
            {
                OrderId = 0,
                ContactName = dto.ContactName!,
                ContactPhone = dto.ContactPhone!,
                ContactEmail = dto.ContactEmail!,
                CustomerNote = dto.CustomerNote,
                TotalAmount = totalAmount,
                CreatedAt = DateTime.Now,
                OrderStatus = "Preview",
                PaymentStatus = "Unpaid",
                Items = new List<OrderItemDetailDto>()
            };

            // 3. 遍歷商品，抓取名稱、日期與「你原本的圖片邏輯」
            foreach (var item in itemsToProcess)
            {
                var productName = await GetProductNameSnapshotAsync(item.ProductCode!);

                var itemDto = new OrderItemDetailDto
                {
                    ProductCode = item.ProductCode!,
                    ProductName = productName,
                    Quantity = item.Quantity,
                    Tickets = new List<OrderTicketDetailDto>()
                };

                // --- 原本的圖片與日期邏輯 ---
                var trip = await _context.TripSchedules
                    .Where(s => s.ProductCode == item.ProductCode)
                    .Select(s => new { s.TripProduct.CoverImage, s.StartDate, s.EndDate })
                    .FirstOrDefaultAsync();

                if (trip != null)
                {
                    itemDto.ProductImage = CartItemDTO.GetFullUrl("/PImages/" + trip.CoverImage, _mvcBaseUrl);
                    itemDto.TripStartDate = trip.StartDate;
                    itemDto.TripEndDate = trip.EndDate;
                }
                else
                {
                    // 嘗試找景點圖片
                    var attr = await _context.AttractionProducts
                        .Where(a => a.ProductCode == item.ProductCode)
                        .Select(a => a.Attraction.Images.Select(img => img.ImagePath).FirstOrDefault())
                        .FirstOrDefaultAsync();

                    if (attr != null)
                    {
                        itemDto.ProductImage = CartItemDTO.GetFullUrl(attr, _mvcBaseUrl);
                    }
                    else
                    {
                        // 嘗試找活動圖片
                        itemDto.ProductImage = await _context.AcitivityTickets
                            .Where(t => t.ProductCode == item.ProductCode)
                            .Select(t => t.CoverImageUrl)
                            .FirstOrDefaultAsync();
                    }
                }

                // --- 票種詳細資訊 ---
                var ticketCat = await _context.TicketCategories.FirstOrDefaultAsync(tc => tc.TicketCategoryId == item.TicketCategoryId);
                var unitPrice = await GetSpecificUnitPriceAsync(item.ProductCode!, (int)item.TicketCategoryId!);

                itemDto.Tickets.Add(new OrderTicketDetailDto
                {
                    TicketName = ticketCat?.CategoryName ?? "一般票",
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice
                });

                preview.Items.Add(itemDto);
            }

            return preview;
        }
        // 3. 我的訂單：查詢該會員的所有訂單簡介 (列表用)
        public async Task<IEnumerable<OrderListDto>> GetMemberOrdersAsync(string memberId)
        {
            // 1. 先抓出該會員的所有訂單主檔
            var orders = await _context.Orders
                .Where(o => o.MemberId == memberId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var resultList = new List<OrderListDto>();

            foreach (var o in orders)
            {
                // 抓取該訂單的第一筆明細，用來當作列表的代表商品
                var firstItem = await _context.OrderItems
                    .Where(oi => oi.OrderId == o.OrderId)
                    .OrderBy(oi => oi.OrderItemId)
                    .FirstOrDefaultAsync();

                string? imageUrl = null;
                string firstItemName = "未知商品";

                if (firstItem != null)
                {
                    firstItemName = firstItem.ProductNameSnapshot ?? "未知商品";

                    // --- 圖片抓取邏輯 (參考你提供的購物車邏輯) ---

                    // 1. 嘗試找行程圖片
                    var trip = await _context.TripSchedules
                        .Where(s => s.ProductCode == firstItem.ProductCode)
                        .Select(s => s.TripProduct.CoverImage)
                        .FirstOrDefaultAsync();

                    if (trip != null)
                    {
                        // 處理路徑：/PImages + 檔名
                        imageUrl = CartItemDTO.GetFullUrl("/PImages/" + trip, _mvcBaseUrl);
                    }
                    else
                    {
                        // 2. 嘗試找景點圖片
                        var attr = await _context.AttractionProducts
                            .Where(a => a.ProductCode == firstItem.ProductCode)
                            .Select(a => a.Attraction.Images.Select(img => img.ImagePath).FirstOrDefault())
                            .FirstOrDefaultAsync();

                        if (attr != null)
                        {
                            imageUrl = CartItemDTO.GetFullUrl(attr, _mvcBaseUrl);
                        }
                        else
                        {
                            // 3. 嘗試找活動圖片 (Cloudinary 網址直接用)
                            var act = await _context.AcitivityTickets
                                .Where(t => t.ProductCode == firstItem.ProductCode)
                                .Select(t => t.CoverImageUrl)
                                .FirstOrDefaultAsync();

                            imageUrl = act; // 活動圖通常是完整網址
                        }
                    }
                }

                resultList.Add(new OrderListDto
                {
                    OrderId = o.OrderId,
                    CreatedAt = (DateTime)o.CreatedAt!,
                    TotalAmount = (decimal)o.TotalAmount!,
                    OrderStatus = o.OrderStatus!,
                    PaymentStatus = o.PaymentStatus!,
                    FirstItemName = firstItemName,
                    FirstItemImage = imageUrl,
                    TotalItemCount = await _context.OrderItems.CountAsync(oi => oi.OrderId == o.OrderId)
                });
            }

            return resultList;

        }
        // 4. 訂單詳情：查看單筆訂單的完整資訊、產品快照、票種明細與圖片
        public async Task<OrderDetailDto?> GetOrderDetailAsync(int orderId, string memberId)
        {
            // 1. 抓取訂單主檔，確保該訂單屬於該會員
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.MemberId == memberId);

            if (order == null) return null;

            var detailDto = new OrderDetailDto
            {
                OrderId = order.OrderId,
                TotalAmount = (decimal)order.TotalAmount!,
                CreatedAt = (DateTime)order.CreatedAt!,
                OrderStatus = order.OrderStatus!,
                PaymentStatus = order.PaymentStatus!,
                ContactName = order.ContactName!,
                ContactPhone = order.ContactPhone!,
                ContactEmail = order.ContactEmail!,
                CustomerNote = order.CustomerNote,
                Items = new List<OrderItemDetailDto>()
            };

            // 2. 抓取所有明細 (OrderItem)
            var items = await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();

            foreach (var item in items)
            {
                var itemDto = new OrderItemDetailDto
                {
                    OrderItemId = item.OrderItemId!,
                    ProductCode = item.ProductCode!,
                    ProductName = item.ProductNameSnapshot!,
                    TripStartDate = item.StartDateSnapshot, // 這裡是 DateOnly?
                    TripEndDate = item.EndDateSnapshot,     // 這裡是 DateOnly?
                    Tickets = new List<OrderTicketDetailDto>()
                };

                // --- 3. 圖片抓取邏輯 (跟列表一致) ---
                var trip = await _context.TripSchedules.Where(s => s.ProductCode == item.ProductCode).Select(s => s.TripProduct.CoverImage).FirstOrDefaultAsync();
                if (trip != null) itemDto.ProductImage = CartItemDTO.GetFullUrl("/PImages/" + trip, _mvcBaseUrl);
                else
                {
                    var attr = await _context.AttractionProducts.Where(a => a.ProductCode == item.ProductCode).Select(a => a.Attraction.Images.Select(img => img.ImagePath).FirstOrDefault()).FirstOrDefaultAsync();
                    if (attr != null) itemDto.ProductImage = CartItemDTO.GetFullUrl(attr, _mvcBaseUrl);
                    else itemDto.ProductImage = await _context.AcitivityTickets.Where(t => t.ProductCode == item.ProductCode).Select(t => t.CoverImageUrl).FirstOrDefaultAsync();
                }

                // --- 4. 抓取該明細下的所有票種 (老人/小孩/學生) ---
                itemDto.Tickets = await _context.OrderItemTickets
                    .Where(oit => oit.OrderItemId == item.OrderItemId)
                    .Select(oit => new OrderTicketDetailDto
                    {
                        TicketName = oit.TicketNameSnapshot!,
                        UnitPrice = (decimal)oit.UnitPrice!,
                        Quantity = (int)oit.Quantity!
                    }).ToListAsync();

                detailDto.Items.Add(itemDto);
            }

            return detailDto;
        }
        //計算錢的方法
        private async Task<decimal> InternalCalculateTotalAsync(List<AddToCartDTO> items)
        {
            // 取得所有要結帳的 ProductCode
            var codes = items.Select(x => x.ProductCode).ToList();

            // 從三張表撈出價格，注意你的 PK 都是 ProductCode (string)
            var priceList = await _context.AcitivityTickets
                .Where(x => codes.Contains(x.ProductCode))
                .Select(x => new { Code = x.ProductCode, Price = (decimal)x.CurrentPrice! })
                .Concat(_context.TripSchedules
                    .Where(x => codes.Contains(x.ProductCode))
                    .Select(x => new { Code = x.ProductCode, Price = (decimal)x.Price! }))
                .Concat(_context.AttractionProducts
                    .Where(x => codes.Contains(x.ProductCode))
                    .Select(x => new { Code = x.ProductCode, Price = (decimal)x.Price! }))
                .ToListAsync();

            // 總金額 = 數量 * 該 Code 對應的單價
            return (from item in items
                    join p in priceList on item.ProductCode equals p.Code
                    select item.Quantity * p.Price).Sum();
        }
        //
        private async Task<string> GetProductNameSnapshotAsync(string code)
        {
            // 1. 先去行程產品表找 (因為行程名稱存在 TripProducts，而 TripSchedules 是子表)
            var tripName = await _context.TripProducts
                .Where(tp => tp.TripSchedules.Any(ts => ts.ProductCode == code))
                .Select(tp => tp.ProductName)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(tripName)) return tripName;

            // 2. 如果行程沒找到，去活動票表找
            var actName = await _context.AcitivityTickets
                .Where(x => x.ProductCode == code)
                .Select(x => x.ProductName)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(actName)) return actName;

            // 3. 最後去景點產品表找 (欄位叫 Title)
            var attName = await _context.AttractionProducts
                .Where(x => x.ProductCode == code)
                .Select(x => x.Title)
                .FirstOrDefaultAsync();

            return attName ?? "未知商品"; // 如果都找不到，給個預設值避免報錯
        }
        private async Task<decimal> GetSpecificUnitPriceAsync(string code, int ticketId)
        {
            // 1. 優先找行程 (TripSchedules)
            var trip = await _context.TripSchedules
                .FirstOrDefaultAsync(ts => ts.ProductCode == code);

            // 注意：你的 trip.Price 是 decimal?，所以要強轉或給預設值
            if (trip != null) return trip.Price ?? 0;

            // 2. 找活動 (AcitivityTickets)
            var act = await _context.AcitivityTickets
                .FirstOrDefaultAsync(x => x.ProductCode == code);

            if (act != null) return (decimal)(act.CurrentPrice ?? 0);

            // 3. 找景點 (AttractionProducts)
            var att = await _context.AttractionProducts
                .FirstOrDefaultAsync(x => x.ProductCode == code);

            return att?.Price ?? 0;
        }
        //針對現有訂單重新取得支付在成立訂單後如果不支付可重新再支付
        public async Task<Order?> GetOrderByIdAsync(int orderId, string memberId)
        {
            return await _context.Orders.AsNoTracking().Include(o=>o.OrderItems).FirstOrDefaultAsync(s => s.OrderId == orderId && s.MemberId == memberId);
        }
    }
}
