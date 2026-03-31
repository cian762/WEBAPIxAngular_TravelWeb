export interface reviewResponseDTO {
  reviewId: number;
  memberId: string;
  title: string;
  comment: string;
  rating: number;
  createDate: Date;
  reviewImages: string[];
  memberAvatar: string;
}
