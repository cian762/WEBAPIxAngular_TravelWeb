export interface ItineraryCreateDto {
  memberId: number;
  itineraryName: string;
  startTime: string;
  endTime: string;
  itemsToPush: any[];
}
