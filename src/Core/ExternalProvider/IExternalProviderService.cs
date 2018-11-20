using System;
using System.Threading.Tasks;

namespace Core.ExternalProvider
{
    public interface IExternalProviderService
    {
        /// <summary>
        ///     Save lykke user id by random guid, during login process through ironclad.
        /// </summary>
        /// <param name="lykkeUserId">Lykke user id.</param>
        /// <param name="ttl">Time to life for temporary data.</param>
        /// <returns>Randomly generated guid, that is saved to cookie during ironclad login through Lykke OAuth.</returns>
        Task<string> SaveLykkeUserIdForExternalLoginAsync(string lykkeUserId, TimeSpan ttl);

        /// <summary>
        ///     Get previously saved lykke user id.
        /// </summary>
        /// <param name="guid">Previously generated guid.</param>
        /// <returns>Saved user id, if it still exists in database. Or empty string if user id not exist.</returns>
        Task<string> GetLykkeUserIdForExternalLoginAsync(string guid);
    }
}
