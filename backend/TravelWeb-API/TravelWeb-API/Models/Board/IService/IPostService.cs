using TravelWeb_API.Models.Board.DbSet;
using TravelWeb_API.Models.Board.DTO;
using TravelWeb_API.Models.MemberSystem;

namespace TravelWeb_API.Models.Board.IService
{
    public interface IPostService
    {
        //Artic(標題物件)
        public Article AddArtic(byte Type,string UserId);
        public Article? GetArticle(int id);
        public Task<bool> UpdateArtic(int id, string? Title, string? PhotoUrl, byte Status);

        //Post(快速發文類)
        public void AddPost(Article article);
        public Task<bool> UpdatePost(int id,string? content, int? regionId,List<string>? photos);

        //(貼文詳情)
        public PostDetailDto GetPostDetailed(Article article);
        public Journal GetJournalDetailed(int id);

        public void SaveChange();
    }
}
