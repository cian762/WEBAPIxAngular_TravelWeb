using TravelWeb_API.Models.Board.DbSet;
using TravelWeb_API.Models.Board.IService;
using TravelWeb_API.Models.MemberSystem;

namespace TravelWeb_API.Models.Board.Service
{
    public class ArticleService : IArticleService
    {
        private readonly BoardDbContext _context;
        private readonly MemberSystemContext _memberDb;
        public ArticleService(BoardDbContext context, MemberSystemContext memberDb)
        {
            _context = context;
            _memberDb = memberDb;
        }

        public List<Article> GetArticles(int page)
        {
            int pageSize = 10;
            // 1. 計算要跳過幾筆
            int skip = (page - 1) * pageSize;
            
            var data = _context.Articles
                .OrderByDescending(a => a.CreatedAt) // 2. 排序：CreatedAt 越新越前 (Descending)
                .Skip((page - 1) * pageSize) // 3. 分頁邏輯
                .Take(pageSize)                
                .ToList();
            
            return data;
        }

        public (List<Article>, int TotalCount) ArticlesByKeyword(int page,string keyword)
        {
            int pageSize = 10;
            // 1. 計算要跳過幾筆
            int skip = (page - 1) * pageSize;
            var data = _context.Articles
                .Where(
                a => (a.Title != null && a.Title.Contains(keyword)) ||
                a.UserId.Contains(keyword)
                ).ToList();

            var result = data.OrderByDescending(a => a.CreatedAt) 
                .Skip((page - 1) * pageSize) 
                .Take(pageSize)
                .ToList();

            return (result,data.Count);
        }

        public (List<Article>, int TotalCount) ArticlesByDate(int page, DateTime startTime,DateTime endTime)
        {
            int pageSize = 10;            
            int skip = (page - 1) * pageSize;
           // startTime = Convert.ToDateTime("2010/05/07");
            endTime = endTime.AddDays(1);

            var data = _context.Articles
                .Where(a => a.CreatedAt >= startTime && 
                      a.CreatedAt <= endTime).ToList();

            var result = data.OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (result,data.Count);
        }
    }
}
