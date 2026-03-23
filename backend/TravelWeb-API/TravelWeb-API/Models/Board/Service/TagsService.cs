using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models.Board.DbSet;
using TravelWeb_API.Models.Board.IService;
using TravelWeb_API.Models.MemberSystem;

namespace TravelWeb_API.Models.Board.Service
{
    public class TagsService:ITagsService
    {

        private readonly BoardDbContext _context;
        private readonly MemberSystemContext _memberDb;
        public TagsService(BoardDbContext context, MemberSystemContext memberDb)
        {
            _context = context;
            _memberDb = memberDb;
        }

        public List<TagsList> getTagsByArticleId(int articleId)
        {
            var data = _context.ArticleTags.Where(t => t.ArticleId == articleId).Include(t => t.Tag);
            var result = data.Select(t => t.Tag).ToList();
            return result;
        }
    }
}
