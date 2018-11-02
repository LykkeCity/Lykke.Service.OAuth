using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Lykke.Common.ApiLibrary.Contract;
using Lykke.Service.OAuth.ApiErrorCodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Lykke.Service.OAuth.Attributes
{
    /// <summary>
    ///     Attribute to validate Public API data models
    /// </summary>
    public class ValidateApiModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionDescriptor is ControllerActionDescriptor descriptor)
            {
                var parameters = descriptor.MethodInfo.GetParameters();

                foreach (var parameter in parameters)
                {
                    var argument = context.ActionArguments.ContainsKey(parameter.Name)
                        ? context.ActionArguments[parameter.Name]
                        : null;

                    EvaluateValidationAttributes(parameter, argument, context.ModelState);
                }
            }

            base.OnActionExecuting(context);

            if (context.ModelState.IsValid) return;

            var apiError = LykkeApiCommonErrorCodes.ModelValidationFailed;
            var message = GetErrorMessage(context.ModelState);

            context.Result = new BadRequestObjectResult(new LykkeApiErrorResponse
            {
                Error = apiError.Name,
                Message = message ?? apiError.DefaultMessage
            });
        }

        private void EvaluateValidationAttributes(ParameterInfo parameter, object argument,
            ModelStateDictionary modelState)
        {
            var validationAttributes = parameter.CustomAttributes;

            foreach (var attributeData in validationAttributes)
            {
                var attributeInstance = parameter.GetCustomAttribute(attributeData.AttributeType);

                if (attributeInstance is ValidationAttribute validationAttribute)
                {
                    var isValid = validationAttribute.IsValid(argument);

                    if (!isValid)
                        modelState.AddModelError(parameter.Name,
                            validationAttribute.FormatErrorMessage(parameter.Name));
                }
            }
        }
        
        //TODO:Remove this method and move to common library. As this is a duplication from APIv2.
        private static string GetErrorMessage(ModelStateDictionary modelStateDictionary)
        {
            var modelError = modelStateDictionary?.Values.FirstOrDefault()?.Errors.FirstOrDefault();

            if (modelError == null)
                return string.Empty;

            return modelError.Exception != null 
                ? modelError.Exception.Message 
                : modelError.ErrorMessage;
        }

    }
}
