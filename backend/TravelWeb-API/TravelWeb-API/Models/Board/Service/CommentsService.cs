using Microsoft.EntityFrameworkCore;
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

            var CList = _context.Comments.Where(c => c.ArticleId == id)
                .Select(c => new CommentsDTO
                {
                    AuthorName = member(c.UserId).Name,
                    AvatarUrl = member(c.UserId).AvatarUrl,
                    Contents = c.Contents,
                    CreatedAt = c.CreatedAt,
                    LikeCount = LikeCount(c),
                    isLiked = false,
                });
            return CList.ToList();
        }


        public Comment AddComment(int articleID, string UserId, string contents)
        {
            Comment comment = new Comment();
            comment.UserId = UserId;
            comment.ParentId = null;
            comment.ArticleId = articleID;
            comment.Contents = contents;
            //comment.CreatedAt = DateTime.Now;

            _context.Comments.Add(comment);
            _context.SaveChanges();
            return comment;
        }

        public Comment AddCommentWithParent(int articleID, string UserId, string contents, int parentID)
        {
            Comment comment = new Comment();
            comment.UserId = UserId;
            comment.ParentId = parentID;
            comment.ArticleId = articleID;
            comment.Contents = contents;
            //comment.CreatedAt = DateTime.Now;

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

        
    }
}
