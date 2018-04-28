using System.Threading.Tasks;

namespace Core.Recaptcha
{
    public interface IRecaptchaService
    {
        Task<bool> Validate(string response = null);
    }
}
