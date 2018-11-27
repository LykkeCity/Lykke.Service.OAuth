using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Services;
using Lykke.Service.OAuth.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.OAuth.Services
{
    public class ExceptionsHandlingConfigurationValidator // : IExceptionsHandlingConfigurationValidator
    {
        private readonly ExceptionsHandlingConfiguration _exceptionsHandlingConfiguration;

        public ExceptionsHandlingConfigurationValidator(ExceptionsHandlingConfiguration exceptionsHandlingConfiguration)
        {
            _exceptionsHandlingConfiguration = exceptionsHandlingConfiguration;
        }

        /// <inheritdoc />
        public IReadOnlyList<Type> Validate()
        {
            IEnumerable<MethodInfo> controllerActions = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(type =>
                    typeof(Controller).IsAssignableFrom(type) || typeof(ControllerBase).IsAssignableFrom(type))
                .SelectMany(type => type.GetMethods())
                .Where(method => method.IsPublic && !method.IsDefined(typeof(NonActionAttribute)));

            List<Type> exceptionsDeclared = controllerActions
                .SelectMany(m => m.GetCustomAttributes(true).OfType<ProducesExceptionTypeAttribute>())
                .Select(x => x.ExceptionType)
                .Distinct()
                .ToList();

            return exceptionsDeclared
                .Where(x => _exceptionsHandlingConfiguration.Find(x) == null)
                .ToList();

            //todo: move to unit test
        }
    }
}
