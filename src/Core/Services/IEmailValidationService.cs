using System.Threading.Tasks;

namespace Core.Services
{
    public interface IEmailValidationService
    {
        Task<bool> IsEmailTakenAsync(string email, string hash);
    }
}
