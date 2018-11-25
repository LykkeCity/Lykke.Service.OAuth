using System.Collections.Generic;
using Autofac;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Messaging;
using Lykke.Messaging.RabbitMq;
using Lykke.Messaging.Serialization;
using Lykke.Service.OAuth.Services;
using Lykke.Service.Registration.Contract.Events;
using Lykke.Service.Salesforce.Contract;
using Lykke.Service.Salesforce.Contract.Commands;
using Lykke.SettingsReader;
using RabbitMQ.Client;
using WebAuth.Settings;

namespace Lykke.Service.OAuth.Modules
{
    public class CqrsModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public CqrsModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            MessagePackSerializerFactory.Defaults.FormatterResolver = MessagePack.Resolvers.ContractlessStandardResolver.Instance;

            builder.RegisterType<AutofacDependencyResolver>().As<IDependencyResolver>().SingleInstance();

            builder.Register(c => new ConnectionFactory { Uri = _settings.CurrentValue.OAuth.Cqrs.RabbitConnectionString });

            builder.RegisterType<RegistrationFinishedProjection>();

            builder.Register(c =>
            {
                var rabbitMqSettings = c.Resolve<ConnectionFactory>();
                return new MessagingEngine(c.Resolve<ILogFactory>(),
                    new TransportResolver(new Dictionary<string, TransportInfo>
                    {
                        {"RabbitMq", new TransportInfo(rabbitMqSettings.Endpoint.ToString(), rabbitMqSettings.UserName, rabbitMqSettings.Password, "None", "RabbitMq")}
                    }),
                    new RabbitMqTransportFactory(c.Resolve<ILogFactory>()));
            });

            builder.Register(ctx => new CqrsEngine(ctx.Resolve<ILogFactory>(),
                    ctx.Resolve<IDependencyResolver>(),
                    ctx.Resolve<MessagingEngine>(),
                    new DefaultEndpointProvider(),
                    true,
                    Register.DefaultEndpointResolver(new RabbitMqConventionEndpointResolver(
                        "RabbitMq",
                        SerializationFormat.MessagePack,
                        environment: "lykke",
                        exclusiveQueuePostfix: "k8s")),

                    Register.BoundedContext("oauth")
                        .ListeningEvents(typeof(RegistrationFinishedEvent))
                        .From("registration").On("events")
                        .WithProjection(typeof(RegistrationFinishedProjection), "registration")
                        .PublishingCommands(
                            typeof(CreateContactCommand),
                            typeof(UpdateContactCommand)
                        )
                        .To(SalesforceBoundedContext.Name)
                        .With("commands")
                ))
                .As<ICqrsEngine>()
                .SingleInstance();
        }
    }
}
