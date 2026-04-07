import { JournalElementDTO } from "./JournalElementDTO";

export interface JournalDetailDTO {
  title: string,
  cover: string,
  createdAt: Date,
  updatedAt: Date,
  status: number,
  avatarUrl: string,
  authorName: string,
  authorID: string,
  regionId: number,
  regionName: string,
  commentCount: number,
  likeCount: number,
  isLike: boolean,
  isCollect: boolean,
  elements: JournalElementDTO[]
}
