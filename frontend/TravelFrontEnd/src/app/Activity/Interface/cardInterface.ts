export interface CardInfoModel {
  activityId: number,
  title: string,
  type: string[],
  region: string[],
  start: Date,
  end: Date,
  coverImageUrl: string,
  referencePrice: number,
  viewCount: number,
  sellCount: number,
  commentCount: number,
  averageRating: number
}
