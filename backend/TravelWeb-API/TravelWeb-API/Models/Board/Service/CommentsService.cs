using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TravelWeb_API.Models.Board.DbSet;
using TravelWeb_API.Models.Board.DTO;
using TravelWeb_API.Models.Board.IService;
using TravelWeb_API.Models.MemberSystem;

namespace TravelWeb_API.Models.Board.Service
{
    public class CommentsService : ICommentsService
    {
        private readonly BoardDbContext _context;
        private readonly MemberSystemContext _memberDb;
        public CommentsService(BoardDbContext context)
        {
            _context = context;
        }

        public List<CommentsDTO>? GetComments(int id)
        {
            if (article(id) == null) return null;

            var commentList = _context.Comments
                .Where(c => c.ArticleId == id)
                .Include(c => c.CommentPhotos)
                .Include(c => c.MemberInformation)
                .Include(c => c.InverseParent).ToList();

            var result = commentList.Where(c => c.ParentId == null).Select(c => new CommentsDTO
            {
                CommentId = c.CommentId,
                AuthorName = c.MemberInformation.Name,
                AvatarUrl = c.MemberInformation.AvatarUrl,
                Contents = c.Contents,
                CreatedAt = c.CreatedAt,
                CommentPhoto = c.CommentPhotos?.FirstOrDefault()?.Photo,
                LikeCount = LikeCount(c),
                ReplyComments = GetReplyComments(c),
                isLiked = false,
            }).ToList();
            return result;
        }


        public Comment AddComment(PostCommentDto dto, string UserId)
        {
            Comment comment = new Comment();
            comment.UserId = UserId;
            comment.ParentId = dto.parentID;
            comment.ArticleId = dto.articleID;
            comment.Contents = dto.contents;
            //comment.CreatedAt = DateTime.Now;


            comment.CommentPhotos = dto.commentPhoto != null
                        ? new List<CommentPhoto> { new CommentPhoto { Photo = dto.commentPhoto } }
                        : new List<CommentPhoto>();
            _context.Comments.Add(comment);
            _context.SaveChanges();
            return comment;
        }

        


        public bool CommentLike(int commentID, string UserId)
        {
            if (member(UserId) == null) return false;//找不到用戶，用戶沒登入
            CommentLike? Like = 
                _context.CommentLikes.FirstOrDefault(c => c.CommentId == commentID && c.UserId == UserId);

            if (Like==null)//沒點讚過，點讚
            {
                _context.CommentLikes.Add
                    (new CommentLike { CommentId = commentID, UserId = UserId });
                _context.SaveChanges();
                return true;
            }
            else//有點讚過，取消點讚
            {
                _context.CommentLikes.Remove(Like);
                _context.SaveChanges();
                return true;
            }
        }

        



        int LikeCount(Comment comment)
        {
            int id = comment.CommentId;
            int Like = _context.CommentLikes.Where(c => c.CommentId == id).Count();
            return Like;
        }

        public bool isLiked(string user, int commentID)
        {
            var like
                = _context.CommentLikes.Any(l => l.UserId == user && l.CommentId == commentID);
            return like;
        }

        Article? article(int id)
        {
            return _context.Articles.FirstOrDefault(a => a.ArticleId == id);
        }

        MemberInformation member(string CommentAuthorID)
        {
            MemberInformation? member =
            _memberDb.MemberInformations.FirstOrDefault(a => a.MemberId == CommentAuthorID);
            return member;
        }

        List<CommentsDTO>? GetReplyComments(Comment comment)
        {
            var result = comment.InverseParent.Select(c => new CommentsDTO
            {
                AuthorName = c.MemberInformation.Name,
                AvatarUrl = c.MemberInformation.AvatarUrl,
                Contents = c.Contents,
                CreatedAt = c.CreatedAt,
                LikeCount = LikeCount(c),
                isLiked = false,
            }).ToList();

            return result;
        }

        public async Task DeleteComment(int commentID, string UserId)
        {
            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.CommentId == commentID);
            if (UserId == comment!.UserId) 
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }
        }
    }
}
