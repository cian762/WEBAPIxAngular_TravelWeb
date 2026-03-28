using System.Threading.Tasks;

namespace TravelWeb_API.Services
{
    public interface IMemberEmailService
    {
        Task SendVerificationCodeAsync(string toEmail, string code);

        Task SendPasswordResetEmailAsync(string toEmail, string code, string resetLink);
    }
}