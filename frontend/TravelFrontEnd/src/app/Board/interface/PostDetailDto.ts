export interface PostDetailDto {
  title?: string,
  type: number,
  cover?: string,
  createdAt: Date,
  updatedAt?: Date,
  contents?: string,
  regionId?: number,
  regionName?: string,
  postPhoto?: string[],
  status: number,
  avatarUrl: string,
  authorName: string,
  authorID: string,
  commentCount: number,
  likeCount: number,
  isLike: boolean,
  isCollect: boolean;






}
