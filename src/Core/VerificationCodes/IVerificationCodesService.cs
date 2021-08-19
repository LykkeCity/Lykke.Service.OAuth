using System.Threading.Tasks;

namespace Core.VerificationCodes
{
    public interface IVerificationCodesService
    {
        Task<VerificationCode> AddCodeAsync(string email, string referer, string returnUrl, string cid, string traffic, string affiliateCode);
        Task<VerificationCode> GetCodeAsync(string key);
        Task<VerificationCode> UpdateCodeAsync(string key);
        Task DeleteCodeAsync(string key);
        Task SetSmsSentAsync(string key);
    }
}
