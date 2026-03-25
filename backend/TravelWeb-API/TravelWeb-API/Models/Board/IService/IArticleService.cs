using TravelWeb_API.Models.Board.DbSet;
using TravelWeb_API.Models.Board.DTO;

namespace TravelWeb_API.Models.Board.IService
{
    public interface IArticleService
    {
        public (List<ArticleDataDTO>, int TotalCount) GetArticles(int page);
        public (List<ArticleDataDTO>, int TotalCount) ArticlesByKeyword(int page,string keyword);
        public (List<ArticleDataDTO>, int TotalCount) ArticlesByDate(int page, DateTime startTime, DateTime endTime);
        public (List<ArticleDataDTO>, int TotalCount) ArticlesByTags(int page, SearchByTagsDTO searchByTags);
        public (List<ArticleDataDTO>, int TotalCount) ArticlesByAuthorID(int page, string authorID);
        public (List<ArticleDataDTO>, int TotalCount) Search(int page, ArticleSearchDTO dto);
    }
}
