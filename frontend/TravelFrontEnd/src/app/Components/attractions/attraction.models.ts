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
  viewCount: number;
  likeCount?: number;
  isLiked?: boolean;
  images?: AttractionImage[];
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
  description: string;
  cities: CityCard[];
}

export interface CityCard {
  name: string;
  regionIds: number[];
  imageUrl: string;
}
