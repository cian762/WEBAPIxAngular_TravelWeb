using TravelWeb_API.Models.Board.DbSet;
using TravelWeb_API.Models.Board.DTO;

namespace TravelWeb_API.Models.Board.IService
{
    public interface IArticleService
    {
        public (List<ArticleDataDTO> ArticleDTOList, int TotalCount) GetArticles(int page,string? userId);
        public (List<ArticleDataDTO> ArticleDTOList, int TotalCount) ArticlesByKeyword(int page,string keyword, string? userId);
        public (List<ArticleDataDTO> ArticleDTOList, int TotalCount) ArticlesByDate(int page, DateTime startTime, DateTime endTime, string? userId);
        public (List<ArticleDataDTO> ArticleDTOList, int TotalCount) ArticlesByTags(int page, SearchByTagsDTO searchByTags, string? userId);
        public (List<ArticleDataDTO> ArticleDTOList, int TotalCount) ArticlesByAuthorID(int page, string authorID, string? userId);
        public (List<ArticleDataDTO> ArticleDTOList, int TotalCount) Search(int page, ArticleSearchDTO dto, string? userId);
        public (List<ArticleDataDTO> ArticleDTOList, int TotalCount) ArticlesByUserID(int page, string userId);
        public (List<ArticleDataDTO> ArticleDTOList, int TotalCount) ArticlesByCollect(int page, string userId);
        public void Like(int articleID, string userId);
        public Task Collect(int articleID, string userId);
        public Task<bool> DeleteArticle(int articleID, string? authorID);
        public (List<ArticleDataDTO> ArticleDTOList, int TotalCount) GetArticlesBySource(int page, string productCode);
        public Task<List<Trending>> GetTrendings();

    }
}
