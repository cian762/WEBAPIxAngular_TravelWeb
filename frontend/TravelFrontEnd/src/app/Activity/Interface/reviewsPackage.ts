import { reviewsInterface } from "./reviewsInterface";

export interface reviewsPackage {
  activityId: number,
  reviews: reviewsInterface[],
  averageRating: number,
  commentCount: number,
}
