// 檔案：Services/IMemberEmailService.cs
using System.Threading.Tasks;

namespace TravelWeb_API.Services
{
    // 獨立的介面，專門給會員系統使用
    public interface IMemberEmailService
    {
        Task SendVerificationCodeAsync(string toEmail, string code);
    }
}