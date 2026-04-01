using TravelWeb_API.Models.Board.DbSet;
using TravelWeb_API.Models.Board.DTO;

namespace TravelWeb_API.Models.Board.IService
{
    public interface ICommentsService
    {
        
        public List<CommentsDTO> GetComments(int id);
        public Comment AddComment(PostCommentDto dto, string UserId);        
        public bool CommentLike(int commentID, string UserId);

        public Task DeleteComment(int commentID, string UserId);

    }
}
