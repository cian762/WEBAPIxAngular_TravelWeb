using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TravelWeb_API.Models.Board.IService;

namespace TravelWeb_API.Models.Board.Service
{
    public class ArticleService : IArticlesService
    {
        private readonly BoardDbContext _context;
        public ArticleService(BoardDbContext context)
        {
            _context = context;
        }

        public Article AddArtic(byte Type,string UserId)
        {
            Article article = new Article();
            article.Type = Type;
            article.CreatedAt = DateTime.Now;
            article.Status = 0;                        
            article.IsViolation = false;
            article.UserId = "";
            _context.Articles.Add(article);
            SaveChange();
            return article;
        }


        public async Task<bool> UpdateArtic(int id,string Title,string PhotoUrl,byte Status)
        {
            Article? article = 
            _context.Articles.FirstOrDefault(a => a.ArticleId == id);
            if (article == null) return false;
            else
            {
                article.Title = Title;
                article.PhotoUrl = PhotoUrl;
                article.Status = Status;                
                article.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }

            
        }
        //article.IsViolation

        public void AddPost(Article article)
        {
            var post = new Post
            {
                // 不要設定 ArticleId = article.ArticleId; 
                Article = article // 直接把物件塞給導覽屬性
            };
            _context.Posts.Add(post);
            SaveChange();
        }

        public void GetArtic(int id)
        {
            throw new NotImplementedException();
        }

        public void SaveChange()
        {
           _context.SaveChanges();
        }

        

        public void UpdatePost(int id)
        {
            throw new NotImplementedException();
        }
    }
}
