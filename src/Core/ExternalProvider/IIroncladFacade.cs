using System.Threading.Tasks;

namespace Core.ExternalProvider
{
    public interface IIroncladFacade
    {
        /// <summary>
        /// Add claim to Ironclad user.
        /// </summary>
        /// <param name="ironcladUserId">Ironclad user id.</param>
        /// <param name="type">Claim tupe.</param>
        /// <param name="value">Claim value.</param>
        /// <returns>Completed task if everything was successful.</returns>
        Task AddUserClaim(string ironcladUserId, string type, string value);
    }
}
