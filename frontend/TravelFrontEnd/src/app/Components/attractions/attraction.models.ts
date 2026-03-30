export interface Attraction {
  attractionId: number;
  name: string;
  address?: string;
  regionId: number;
  regionName?: string;
  latitude?: number;
  longitude?: number;
  phone?: string;
  website?: string;
  businessHours?: string;
  closedDaysNote?: string;
  transportInfo?: string;
  description?: string;      // 景點介紹文字
  viewCount: number;
  likeCount?: number;
  isLiked?: boolean;
  mainImage?: string;        // ← 加這行（列表頁用）
  images?: string[];//images?: AttractionImage[];改成images?: string[];

  types?: AttractionType[];
}

export interface AttractionImage {
  imageId: number;
  imagePath: string;
  attractionId: number;
}

export interface AttractionType {
  attractionTypeId: number;
  attractionTypeName: string;
}

export interface RegionGroup {
  label: string;
  mapImage: string;   // ← 加這行
  description: string;
  cities: CityCard[];
}

export interface CityCard {
  name: string;
  regionIds: number[];
  imageUrl: string;
}
