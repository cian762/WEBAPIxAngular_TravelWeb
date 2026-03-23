export interface ItineraryCreateDto {
  memberId: number;
  itineraryName: string;
  startTime: string;
  endTime: string;
  itemsToPush: any[];
}
export interface ItineraryItem {
  itemId: number;
  sortOrder: number;
  contentDescription: string;
  attractionName: string;
  address: string;
  latitude?: number;
  longitude?: number;
  startTime?: string;
  endTime?: string;
}

export interface DayPlan {
  day: number;
  items: ItineraryItem[];
}
