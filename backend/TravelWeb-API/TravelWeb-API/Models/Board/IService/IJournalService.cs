using TravelWeb_API.Models.Board.DbSet;

namespace TravelWeb_API.Models.Board.IService
{
    public interface IJournalService
    {
        public Task<bool> postJournal(string userId); 
    }
}
