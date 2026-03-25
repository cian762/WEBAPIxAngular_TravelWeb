using Azure.Core;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;

namespace TravelWeb_API.Controllers.Board
{
    public class GetUser
    {
        //創建 SingleObject 的一個對象
        private static GetUser instance = new GetUser();

        //讓構造函數為 private，這樣該類就不會被實例化
        private GetUser() { }

        //獲取唯一可用的對象
        public static string? Id(string? token)
        {
            if (token == null) return null;
            // 解碼 JWT
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // 取得 Payload 裡的資料
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "MemberId")?.Value;
            return userId;
        }

        
    }
}
