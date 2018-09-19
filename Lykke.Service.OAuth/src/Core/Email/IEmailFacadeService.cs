    using System.Threading.Tasks;

namespace Core.Email
{
    public interface IEmailFacadeService
    {
        Task SendVerifyCode(string email, string code, string url);
    }
}
