using System.Threading.Tasks;
using Core.Exceptions;

namespace Core.Registration
{
    /// <summary>
    /// Repository to store registration flow state
    /// </summary>
    public interface IRegistrationRepository
    {
        /// <summary>
        /// Add new registration
        /// </summary>
        /// <param name="registrationModel"></param>
        /// <returns>Registration id</returns>
        Task<string> AddAsync(RegistrationModel registrationModel);

        /// <summary>
        /// Update existing registration
        /// </summary>
        /// <param name="registrationModel"></param>
        /// <returns>Registration id</returns>
        Task<string> UpdateAsync(RegistrationModel registrationModel);

        /// <summary>
        /// Get registration model by id
        /// </summary>
        /// <param name="registrationId">Registration id</param>
        /// <returns>Registration model</returns>
        /// <exception cref="RegistrationKeyNotFoundException">Thrown when registration id is not found in repository</exception>
        Task<RegistrationModel> GetByIdAsync(string registrationId);

        /// <summary>
        /// Get registration model by email
        /// </summary>
        /// <param name="email">Registration email</param>
        /// <returns>Registration model or null if not found</returns>
        Task<RegistrationModel> GetByEmailAsync(string email);
    }
}
