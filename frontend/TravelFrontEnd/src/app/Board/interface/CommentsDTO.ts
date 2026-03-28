export interface CommentsDTO {
  commentId: number,
  authorName: string,
  avatarUrl?: string,
  contents: string,
  createdAt: Date,
  likeCount: number,
  isLiked: boolean,
  commentPhoto?: string,
  replyComments: CommentsDTO[]
}
