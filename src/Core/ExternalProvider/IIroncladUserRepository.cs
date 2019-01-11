using System.Threading.Tasks;

namespace Core.ExternalProvider
{
    public interface IIroncladUserRepository
    {
        /// <summary>
        ///     Add binding between ironclad user and lykke user.
        /// </summary>
        /// <param name="ironcladUserBinding">Ironclad user.</param>
        /// <returns>True if binding was added successfully.</returns>
        Task<bool> AddAsync(IroncladUserBinding ironcladUserBinding);

        /// <summary>
        ///     Get associated user.
        /// </summary>
        /// <param name="ironcladUserId">Ironclad user id.</param>
        /// <returns>Associated user.</returns>
        Task<IroncladUserBinding> GetByIdAsync(string ironcladUserId);
    }
}
