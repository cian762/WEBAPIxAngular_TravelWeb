export interface routeRequestInterface {
  originLat: number;
  originLng: number;
  destinationLat: number;
  destinationLng: number;
  travelMode: 'DRIVE' | 'WALK' | 'BICYCLE' | 'TRANSIT';
}

