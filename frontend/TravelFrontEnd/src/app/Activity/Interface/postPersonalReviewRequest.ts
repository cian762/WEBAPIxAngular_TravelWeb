export interface postPersonalReviewRequest {
  activityId: number;
  title: string;
  comment: string;
  rating: number;
  reviewImages: File[];
}
