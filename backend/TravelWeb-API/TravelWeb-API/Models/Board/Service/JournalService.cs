using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Drawing;
using TravelWeb_API.Models.Board.DbSet;
using TravelWeb_API.Models.Board.DTO;
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

        public async Task<JournalUpdateDTO> UpdateJournal(int articleId)
        {
            JournalUpdateDTO? result = await _context.Articles
                .Where(a => a.ArticleId == articleId)
                .Select(a => new JournalUpdateDTO
                {
                    Cover = a.PhotoUrl,
                    Title = a.Title,
                    Status = a.Status,
                    RegionId = a.RegionID
                })
                .FirstOrDefaultAsync();

            List<JournalElementDTO> Elements = new List<JournalElementDTO>();
            var Journals = await _context.JournalElements
                .Where(j => j.ArticleId == articleId).ToListAsync();
            foreach (var j in Journals)
            {
                var DTO = new JournalElementDTO
                {
                    Page = j.Page,
                    PosX = j.PosX,
                    PosY = j.PosY,
                    Rotation = j.Rotation,
                    Zindex = j.Zindex,
                    ElementType = j.ElementType,
                    content = j.Photo,
                    Width = j.Width,
                    Height = j.Height,

                };
                Elements.Add(DTO);
            }

            result.Elements = Elements;


            return result;
        }

        public async Task<JournalDetailDTO> GetJournalDetail(int articleId,string currentUserId)
        {
            JournalDetailDTO DTO = new JournalDetailDTO();
             DTO = await _context.Articles.Where(a=>a.ArticleId==articleId)                
                .Select(a=>new JournalDetailDTO { 
                    Title = a.Title,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt,
                    Cover = a.PhotoUrl,                   
                    RegionId = a.RegionID,
                    RegionName = a.Region != null ? a.Region.RegionName : null,                   
                    AuthorID = a.UserId,
                    AuthorName = a.MemberInformation.Name,
                    AvatarUrl = a.MemberInformation.AvatarUrl,
                    Status = a.Status,
                    CommentCount = a.Comments.Count,
                    LikeCount = a.ArticleLikes.Count,                    
                    isLike = a.ArticleLikes.Any(l => l.UserId == currentUserId),
                    isCollect = a.ArticleFolders.Any(c => c.UserId == currentUserId),

                }).FirstAsync();

            DTO.Elements = await _context.JournalElements.Where(j => j.ArticleId == articleId)
                .Select(j=> new JournalElementDTO
                {
                    Page = j.Page,
                    PosX = j.PosX,
                    PosY = j.PosY,
                    Rotation = j.Rotation,
                    Zindex = j.Zindex,
                    ElementType = j.ElementType,
                    content = j.Photo,
                    Width = j.Width,
                    Height = j.Height,

                }).ToListAsync();



            return DTO;
        }

        public Task<bool> isAuthor(int articleId, string currentUserId)
        {
            throw new NotImplementedException();
        }

        public async Task<int> postJournal(string userId)
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
            await _context.SaveChangesAsync();
            return article.ArticleId;

        }

        public async Task<bool> putJournal(int articleId, JournalUpdateDTO updateDTO)
        {
            try
            {
                Article? article = await _context.Articles
                    .FirstOrDefaultAsync(a => a.ArticleId == articleId);
                if (article == null) return false;                
                article.Title = updateDTO.Title;
                article.PhotoUrl = updateDTO.Cover;
                article.Status = updateDTO.Status;
                article.UpdatedAt = DateTime.Now;
                article.RegionID = updateDTO.RegionId;

                var oldEl = _context.JournalElements.Where(el => el.ArticleId == articleId);
                _context.JournalElements.RemoveRange(oldEl);

                foreach (var e in updateDTO.Elements)
                {
                    JournalElement journalElement = new JournalElement
                    {
                        ArticleId = articleId,
                        Page = e.Page,
                        PosX = e.PosX,
                        PosY = e.PosY,
                        Rotation = e.Rotation,
                        Zindex = e.Zindex,
                        ElementType = e.ElementType,
                        Photo = e.content,
                        Width = e.Width,
                        Height = e.Height,

                    };
                    await _context.JournalElements.AddAsync(journalElement);
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex) { return false; }
        }

        
    }
}
