import { routeStepDto } from "./routeStepDto";

export interface RouteOptionDto {
  summary: string;
  distanceText: string;
  durationText: string;
  distanceMeters: number;
  encodedPolyline: string;
  routeLabels: string[];
  steps: routeStepDto[];
}
