export interface ItineraryCreateDto {
  memberId: number;
  itineraryName: string;
  startTime: string;
  endTime: string;
  itemsToPush: any[];
}
export interface ItineraryItem {
  isTimePickerOpen?: boolean;
  itemId: number;
  dayNumber: number;
  attractionId: number;
  placeId: string;
  googlePlaceId: string;
  sortOrder: number;
  contentDescription: string;
  attractionName: string;
  address: string;
  latitude?: number;
  longitude?: number;
  startTime?: string;
  endTime?: string;
  isEditing?: boolean
  editingName?: string
}

export interface DayPlan {
  day: number;
  items: ItineraryItem[];
}
export interface DayItineraryItemDto {
  order?: number;
  placeId?: string;
  placeName?: string;
  attractionName?: string;   // 前端顯示用，後端不一定有
  address?: string;
  startTime?: string;        // 對應後端 ArrivalTime，格式 "HH:mm:ss"
  endTime?: string;          // 對應後端 DepartureTime
}

export interface DayItineraryDto {
  dayNumber: number;
  items: DayItineraryItemDto[];
}
