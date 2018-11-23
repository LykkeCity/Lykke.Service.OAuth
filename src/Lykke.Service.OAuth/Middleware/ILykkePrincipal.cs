using System.Security.Claims;
using System.Threading.Tasks;

namespace Lykke.Service.OAuth.Middleware
{
    internal interface ILykkePrincipal
    {
        Task<ClaimsPrincipal> GetCurrent();
        string GetToken();
    }
}
