export interface editPersonalReviewRequest {
  reviewId: number;
  title: string;
  comment: string;
  rating: number;
  newImages: File[];
  deletedImageUrls: string[];
}
