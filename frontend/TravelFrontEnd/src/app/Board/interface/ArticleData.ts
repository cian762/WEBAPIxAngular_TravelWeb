import { Data } from "@angular/router";

export interface ArticleResponse {
  totalCount: number;
  articleList: ArticleData[];
}
export interface ArticleData {
  articleId: number;
  title?: string;
  createdAt: Date;
  photoUrl?: string;
  userID: string;
  userName?: string;
  userAvatar?: string;
  tags?: TagDTO[]
}

export interface TagDTO {
  TagId: number;
  TagName: string;
}
