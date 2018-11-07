using System.Threading.Tasks;
using Core.Exceptions;
using Core.Registration;

namespace Core.Services
{
    /// <summary>
    /// The handler for account info registration step
    /// </summary>
    public interface IAccountInfoStepHandler
    {
        /// <summary>
        /// Handles the step of registration
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="RegistrationKeyNotFoundException">Registration id is not found</exception>
        /// <exception cref="CountryFromRestrictedListException">Country provided is from restricted country list</exception>
        /// <exception cref="InvalidPhoneNumberFormatException">Invalid phone number format</exception>
        /// <exception cref="PhoneNumberAlreadyInUseException">Phone number already used</exception>
        Task HandleAsync(AccountInfoDto model);
    }
}
