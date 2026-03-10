namespace TravelWeb_API.Models.Board.IService
{
    public interface IArticlesService
    {
        //Artic(標題物件)
        public Article AddArtic(byte Type,string UserId);
        public void GetArtic(int id);
        public Task<bool> UpdateArtic(int id, string Title, string PhotoUrl, byte Status);

        //Post(快速發文類)
        public void AddPost(Article article);
        public void UpdatePost(int id);



        public void SaveChange();
    }
}
