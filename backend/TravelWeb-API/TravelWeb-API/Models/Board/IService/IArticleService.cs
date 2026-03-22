using TravelWeb_API.Models.Board.DbSet;

namespace TravelWeb_API.Models.Board.IService
{
    public interface IArticleService
    {
        public List<Article> GetArticles(int page);
        public (List<Article>, int TotalCount) ArticlesByKeyword(int page,string keyword);
        public (List<Article>, int TotalCount) ArticlesByDate(int page, DateTime startTime, DateTime endTime);
    }
}
