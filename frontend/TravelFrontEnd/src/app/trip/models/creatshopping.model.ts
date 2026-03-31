export interface CartItem {
  cartId: number;
  productCode: string;
  productName: string;
  price: number;
  quantity: number;
  coverImage: string;
  ticketCategoryId: number;
    attractionId?: number;   // 用於編輯按鈕導回景點售票區
    attractionName?: string;  // 景點名稱
  tags?: string[];          // tag 名稱列表（用於顯示免費取消等）
}
