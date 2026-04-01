using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models.Board.DbSet;
using TravelWeb_API.Models.Board.IService;
using TravelWeb_API.Models.MemberSystem;

namespace TravelWeb_API.Models.Board.Service
{
    public class JournalService : IJournalService
    {
        private readonly BoardDbContext _context;        
        public JournalService(BoardDbContext context)
        {
            _context = context;
            
        }

        public async Task<bool> postJournal(string userId)
        {
            Article article = new Article();    
            article.UserId = userId;
            article.Type = 1;            
            article.CreatedAt = DateTime.Now;
            article.Status = 0;
            article.IsViolation = false;
            article.Journals.Add(new Journal 
            { 
            Page = 1,
            });
            await _context.Articles.AddAsync(article);
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }
    }
}
