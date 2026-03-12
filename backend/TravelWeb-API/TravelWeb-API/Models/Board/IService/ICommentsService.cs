using TravelWeb_API.Models.Board.DbSet;
using TravelWeb_API.Models.Board.DTO;

namespace TravelWeb_API.Models.Board.IService
{
    public interface ICommentsService
    {
        
        public List<CommentsDTO> GetComments(int id);
        public Comment AddComment(int articleID, string UserId, string contents);
        public Comment AddCommentWithParent(int articleID, string UserId, string contents, int parentID);
        public bool CommentLike(int commentID, string UserId);

    }
}
