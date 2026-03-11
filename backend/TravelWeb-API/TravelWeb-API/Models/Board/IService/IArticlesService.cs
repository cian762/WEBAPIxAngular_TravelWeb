using TravelWeb_API.Models.Board.DTO;

namespace TravelWeb_API.Models.Board.IService
{
    public interface IArticlesService
    {
        //Artic(標題物件)
        public Article AddArtic(byte Type,string UserId);
        public Article? GetArtic(int id);
        public Task<bool> UpdateArtic(int id, string Title, string PhotoUrl, byte Status);

        //Post(快速發文類)
        public void AddPost(Article article);
        public void UpdatePost(int id);

        //(貼文詳情)
        public PostDetailDto GetPostDetailed(Article article);
        public Journal GetJournalDetailed(int id);


        //留言，以下記得拆家
        public List<CommentsDTO> GetComments(int id);
        public Comment AddComment(int articleID, string UserId, string contents);

        public void SaveChange();
    }
}
