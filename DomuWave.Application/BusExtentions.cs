
using CPQ.Core.Bus;
using CPQ.Core.Settings;
using CPQ.Core.Startups;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


//using OMX.Services.Consumers.Bus.OMX;


namespace Auth.Microservice
{
    /// <summary>
    /// </summary>
    public static class BusExtentions
    {
        /// <summary>
        ///     add omx bus configurations
        /// </summary>
        /// <param name="services"></param>
        /// <param name="env"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddAuthlBus(this IServiceCollection       services, IWebHostEnvironment env,
                                                   OxCoreSettings.RabbitMQSettings configuration)
        {
            var prefix = $"{BusConfiguration.QueuePrefix("x_auth", env.EnvironmentName, env.IsProduction())}";


            var name = $"[{env.EnvironmentName.ToLowerInvariant()}]-[x_auth]-";


            var busConfiguration = BusConfiguration.Create(configuration).SetPrefixTemporaryQueueName(name);


            void _intBus(IBusRegistrationContext context, IBusFactoryConfigurator configurator)
            {

                busConfiguration.BuildBasicRabbitMqConfigurator(configurator);

                configurator.UseKillSwitch(options => options
                                                      .SetActivationThreshold(10)
                                                      .SetTripThreshold(0.15)
                                                      .SetRestartTimeout(m: 1));

                //configurator.AddOxCoreAuthBusConfigurations(prefix, context);
                configurator.AddOxCoreBusConfigurations(prefix, context);


                //auth bus endopint
               








                // usingRabbitMq.ConnectConsumeObserver(new OxCoreConsumeObserver(serviceProvider));

            }
            services.AddMassTransit(e =>
            {
                // bus
                e.UsingRabbitMq(_intBus);
                // add core auth 
                //e.AddOxAuthCoreConsumers();
                // add core consumer 
                e.AddOxCoreConsumers();

             





            });


            return services;
        }
    }
}