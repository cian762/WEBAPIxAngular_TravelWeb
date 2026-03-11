using TravelWeb_API.Models.TripProduct.TripDTO;

namespace TravelWeb_API.Models.TripProduct.ITripProduct
{
    public interface IShoppingCart
    {
        // 1. 取得某會員的購物車內容 (要包含產品細節如名稱、價格、圖片)
        Task<IEnumerable<CartItemDTO>> GetCartAsync(string memberId);

        // 2. 加入購物車 (如果已存在則加數量，不存在則新增)
        Task AddToCartAsync(AddToCartDTO dto);

        // 3. 更新購物車數量 (例如在購物車頁面加減數量)
        Task UpdateQuantityAsync(UpdateCartQtyDTO dto);

        // 4. 刪除購物車單一項目
        Task RemoveItemAsync(int cartId);

        // 5. 清空購物車
        Task ClearCartAsync(string memberId);




    }
}
