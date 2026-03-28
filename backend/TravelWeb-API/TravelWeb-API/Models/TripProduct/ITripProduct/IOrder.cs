using TravelWeb_API.Models.TripProduct.TripDTO;

namespace TravelWeb_API.Models.TripProduct.ITripProduct
{
    public interface IOrder
    {
        // 1. 建立訂單：處理 4 張表寫入與 Transaction，回傳產生的 OrderId
        Task<Order>CreateOrderAsync(CreateOrderDto dto, string memberId);

        // 2. 訂單預覽：在正式下單前，計算金額與確認商品清單 (不寫入資料庫)
        Task<OrderDetailDto> GetCheckoutPreviewAsync(CreateOrderDto dto, string memberId);

        // 3. 我的訂單：查詢該會員的所有訂單簡介 (列表用)
        Task<IEnumerable<OrderListDto>> GetMemberOrdersAsync(string memberId);

        // 4. 訂單詳情：查看單筆訂單的完整資訊、產品快照、票種明細與圖片
        Task<OrderDetailDto?> GetOrderDetailAsync(int orderId, string memberId);

        // 5. 取消訂單：僅限未付款且 Pending 狀態，更新狀態為 Cancelled
        Task<bool> CancelOrderAsync(int orderId, string memberId);
        //針對現有訂單重新取得支付參數
        Task<Order?> GetOrderByIdAsync(int orderId, string memberId);

    }
}
