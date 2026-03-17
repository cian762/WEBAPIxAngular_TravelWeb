export interface PostDetailDto {
  title: string;
  type: number;
  cover: string;
  createdAt: Date;
  updatedAt: Date;
  contents: string;
  regionId: number;
  avatarUrl: string;
  authorName: string;
}
