using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Core.ExternalProvider;

namespace Lykke.Service.OAuth.ExternalProvider
{
    public interface IIroncladUtils
    {
        /// <summary>
        ///     Get ironclad openid tokens from authentication response
        /// </summary>
        /// <returns>ironclad openid tokens</returns>
        Task<OpenIdTokens> GetIroncladTokensAsync();

        /// <summary>
        ///     Save logout request
        /// </summary>
        /// <param name="request">original logout request</param>
        void SaveIroncladLogoutContext(OpenIdConnectRequest request);

        /// <summary>
        ///     Get saved logout request
        /// </summary>
        /// <returns>saved logout request</returns>
        OpenIdConnectRequest GetIroncladLogoutContext();

        /// <summary>
        ///     Clear logout request
        /// </summary>
        void ClearIroncladLogoutContext();
    }
}
