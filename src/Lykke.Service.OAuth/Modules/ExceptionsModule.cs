using System.Net;
using Autofac;
using Core.Exceptions;
using Lykke.Service.OAuth.ApiErrorCodes;

namespace Lykke.Service.OAuth.Modules
{
    public class ExceptionsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var config = new ExceptionsHandlingConfiguration()

                .AddWarning(typeof(EmailHashInvalidException), HttpStatusCode.BadRequest,
                    RegistrationErrorCodes.InvalidBCryptHash)

                .AddWarning(typeof(BCryptWorkFactorOutOfRangeException), HttpStatusCode.BadRequest,
                    RegistrationErrorCodes.BCryptWorkFactorOutOfRange);

            builder.Register(c => config)
                .AsSelf()
                .SingleInstance();
        }
    }
}
