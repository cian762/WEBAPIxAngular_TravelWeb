export interface PostDetailDto {
  title?: string,
  type: number,
  cover?: string,
  createdAt: Date,
  updatedAt?: Date,
  contents?: string,
  regionId?: number,
  postPhoto?: string[],
  status: number,
  avatarUrl: string,
  authorName: string
}
