using System;
using System.Collections.Generic;

namespace Core.Services
{
    public interface IExceptionsHandlingConfigurationValidator
    {
        /// <summary>
        /// Validates the exceptions handling configuration
        /// </summary>
        /// <returns>List of exception types which are described in controller actions using <see cref="ProducesExceptionType"/> but configuration is missed</returns>
        IReadOnlyList<Type> Validate();
    }
}
