using System.Collections.Generic;
using System.Linq;
using Lykke.Common.ApiLibrary.Contract;
using Lykke.Service.OAuth.Attributes;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.OAuth.Swagger
{
    public class ErrorDescriptionOperationFilter : IOperationFilter
    {
        private readonly ExceptionsHandlingConfiguration _exceptionsHandlingConfiguration;

        public ErrorDescriptionOperationFilter(ExceptionsHandlingConfiguration exceptionsHandlingConfiguration)
        {
            _exceptionsHandlingConfiguration = exceptionsHandlingConfiguration;
        }

        public void Apply(Operation operation, OperationFilterContext context)
        {
            var exceptionAttributes = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<ProducesExceptionTypeAttribute>()
                .ToList();

            if (!exceptionAttributes.Any()) return;

            var configurations = exceptionAttributes
                .Select(x => _exceptionsHandlingConfiguration.Find(x.ExceptionType))
                .Where(x => x != null)
                .ToList();

            var configurationsByStatusCode = configurations.GroupBy(x => x.ResponseStatusCode).OrderBy(x => x.Key);

            foreach (var statusCodeGroup in configurationsByStatusCode)
            {
                var statusCode = ((int) statusCodeGroup.Key).ToString();

                var appErrorCodeNames = statusCodeGroup.ToList().Select(x => x.ResponseErrorCode.Name).Distinct();

                string description =
                    $"Status code: {statusCodeGroup.Key}. Application error codes: {string.Join(", ", appErrorCodeNames)}.";

                var response = new Response
                {
                    Description = description,
                    Schema = new Schema {Example = new LykkeApiErrorResponse {Error = "string", Message = "string"}}
                };

                if (!operation.Responses.TryAdd(statusCode, response))
                {
                    operation.Responses[statusCode] = response;
                }
            }
        }
    }
}
