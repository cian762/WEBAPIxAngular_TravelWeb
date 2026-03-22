// TripProductDTO.ts
export interface TripProductDTO {
  tripProductId: number;      // 注意：前端習慣用小駝峰 (lowerCamelCase)
  productName?: string;       // TypeScript 使用 ?: 表示可空
  coverImage?: string;
  displayPrice?: number;      // C# 的 decimal 在 TS 對應 number
  regionName?: string;
  categoryTag?: string;
  durationDays?: number;
}

// 搜尋條件也要準備好
export interface ProductQueryDTO {
  tagIds?: number[];          // 陣列型別
  regionId?: number;
  keyword?: string;
  minPrice?: number;
  maxPrice?: number;
  page: number;               // 分頁通常給預設值，不一定要可空
  pageSize: number;
  sortType?: number;
}

// 分頁回傳的包裝 (對應你後端的 PagedResult)
export interface PagedResult<T> {
  totalCount: number;
  data: T[];
}
export interface RegionListDTO {
  regionId: number;
  regionName: string;
}
export interface TagListDTO {
  tagId: number;
  tagName: string;
}
// 1. 商品基本資訊
export interface ProductBasic {
  tripProductId: number;
  productName: string;
  coverImage: string | null;
  description: string | null;
  regionName: string;
  tags: string[];
}

// 2. 出發日期與價格
export interface ProductSchedule {
  productCode: string;
  startDate: string; // 注意：API 回傳的 DateOnly/DateTime 在前端會變成 ISO 字串
  price: number;
  availableStock: number;
}

// 3. 行程細節 (包含多圖輪播)
export interface ProductItinerary {
  dayNumber: number;
  sortOrder: number;
  defaultDescription: string | null;
  customText: string | null;
  resourceName: string | null;
  resourceUrls: string[]; // 對應後端的 List<string>
}
