/** 建立訂單與預覽共用的 DTO */
export interface CreateOrderDto {
  contactName: string;
  contactPhone: string;
  contactEmail: string;
  customerNote?: string;
  // 商品來源：可以是購物車，也可以是直接購買的項目
  directBuyItems?: AddToCartDto[];
}
/** 購物車/直接購買項目的簡潔格式 */
export interface AddToCartDto {
  productCode: string;
  quantity: number;
  ticketCategoryId: number;
}
/** 預覽訂單時後端回傳的格式 (OrderDetailDto) */
export interface OrderDetailDto {
  orderId: number;
  contactName: string;
  contactPhone: string;
  contactEmail: string;
  customerNote: string;
  totalAmount: number;
  createdAt: Date;
  orderStatus: string;
  paymentStatus: string;
  items: OrderItemDetailDto[];
}
/** 訂單內的單項商品明細 */
export interface OrderItemDetailDto {
  productCode: string;
  productName: string;
  productImage: string;
  quantity: number;
  tripStartDate?: Date;
  tripEndDate?: Date;
  tickets: OrderTicketDetailDto[];
}
/** 票種詳細資訊 */
export interface OrderTicketDetailDto {
  ticketName: string;
  quantity: number;
  unitPrice: number;
}
