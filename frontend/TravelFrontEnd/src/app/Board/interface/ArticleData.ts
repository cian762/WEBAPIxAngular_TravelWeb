import { Data } from "@angular/router";

export interface ArticleResponse {
  totalCount: number;
  articleList: ArticleData[];
}
export interface ArticleData {
  articleId: number;
  type: number;
  title?: string;
  createdAt: Date;
  photoUrl?: string;
  userID: string;
  userName?: string;
  userAvatar?: string;
  likeCount: number;
  isLike: boolean;
  regionName?: string;
  commentCount: number;
  tags?: TagDTO[];
  viewCount: number;

}

export interface TagDTO {
  tagId: number;
  tagName: string;
  icon: string;
}
