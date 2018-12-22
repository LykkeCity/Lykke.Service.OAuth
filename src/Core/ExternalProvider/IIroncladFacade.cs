using System.Threading.Tasks;
using Core.ExternalProvider.Exceptions;

namespace Core.ExternalProvider
{
    public interface IIroncladFacade
    {
        /// <summary>
        ///     Add claim to Ironclad user.
        /// </summary>
        /// <param name="ironcladUserId">Ironclad user id.</param>
        /// <param name="type">Claim tupe.</param>
        /// <param name="value">Claim value.</param>
        /// <returns>Completed task if everything was successful.</returns>
        /// <exception cref="AuthenticationException">Thrown when ironclad api access token could not retrieved.</exception>
        Task AddUserClaimAsync(string ironcladUserId, string type, string value);
    }
}
