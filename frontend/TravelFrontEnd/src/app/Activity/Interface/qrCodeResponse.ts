export interface qrCodeResponse {
  qrToken: string;
  productCode: string;
  productName: string;
  message: string;
  expiredDate: Date;
  useAt: Date;
  qrCodeImage: string;
}
