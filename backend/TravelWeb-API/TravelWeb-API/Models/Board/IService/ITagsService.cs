using System.Diagnostics;
using TravelWeb_API.Models.Board.DbSet;

namespace TravelWeb_API.Models.Board.IService
{
    public interface ITagsService
    {
        public List<TagsList> getTagsByArticleId(int articleId);
    }
}
