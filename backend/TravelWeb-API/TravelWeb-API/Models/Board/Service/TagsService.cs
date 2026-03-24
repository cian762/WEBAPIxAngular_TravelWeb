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

        public bool EditTagsByArticleId(int articleId, List<int>? TagIDList)
        {
            try
            {
                var removeList = _context.ArticleTags.Where(t => t.ArticleId == articleId);
                _context.ArticleTags.RemoveRange(removeList);
                if (TagIDList == null) return true;
                var tagsToAdd = new List<ArticleTag>();
                foreach (var tagId in TagIDList)
                {
                    tagsToAdd.Add(new ArticleTag
                    {
                        ArticleId = articleId,
                        TagId = tagId
                    });
                }
                _context.ArticleTags.AddRange(tagsToAdd);
                _context.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        public List<TagsList> getAllTags()
        {
           return _context.TagsLists.ToList();
        }

        public List<TagsList> getTagsByArticleId(int articleId)
        {
            var data = _context.ArticleTags.Where(t => t.ArticleId == articleId).Include(t => t.Tag);
            var result = data.Select(t => t.Tag).ToList();
            return result;
        }
    }
}
