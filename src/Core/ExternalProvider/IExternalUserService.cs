using System.Threading.Tasks;
using Core.Exceptions;

namespace Core.ExternalProvider
{
    /// <summary>
    ///     External user operations.
    /// </summary>
    public interface IExternalUserService
    {
        /// <summary>
        ///     Associate lykke user with external user.
        /// </summary>
        //TODO:@gafanasiev Think if we need "provider" here. Or we can use ironclad for associating any account.
        /// <param name="provider">External provider name.</param>
        /// <param name="externalUserId">External user id.</param>
        /// <param name="lykkeUserId">Lykke user id.</param>
        /// <returns>Completed Task if user was associated.</returns>
        /// <exception cref="ExternalUserAlreadyAssociatedException">Thrown when external user already associated with lykke user.</exception>
        Task AssociateExternalUserAsync(string provider, string externalUserId, string lykkeUserId);

        /// <summary>
        ///     Get associated lykke user id.
        /// </summary>
        /// <param name="provider">External provider name.</param>
        /// <param name="externalUserId">External user id.</param>
        /// <returns>
        ///     Lykke user id if it was already associated.
        ///     Empty string if user is not associated.
        /// </returns>
        Task<string> GetAssociatedLykkeUserIdAsync(string provider, string externalUserId);
    }
}
