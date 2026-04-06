using TravelWeb_API.Models.Board.DbSet;
using TravelWeb_API.Models.Board.DTO;

namespace TravelWeb_API.Models.Board.IService
{
    public interface IJournalService
    {
        public Task<int> postJournal(string userId);
        public Task<bool> putJournal(int articleId, JournalUpdateDTO updateDTO);
        public Task<bool> isAuthor(int articleId,string currentUserId);
        public Task<JournalDetailDTO?> GetJournalDetail(int articleId,string currentUserId);
        public Task<JournalUpdateDTO> UpdateJournal(int articleId);
    }
}
