export interface ticketInfoInterface {
  productCode: string;
  productName: string;
  description: string;
  termsOfService: string;
  coverImageUrl: string;
  notes: string;
  ticketCategoryId: number;
  startDate: Date;
  expiryDate: Date;
  currentPrice: number;
  ticketStock: number;
  tag: string[];
}
