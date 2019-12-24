using Blockcore.Platform;
using Blockcore.Platform.Networking;
using Blockcore.Platform.Networking.Actions;
using Blockcore.Platform.Networking.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;

namespace Blockcore.Hub
{
    public class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddHostedService<HubWorker>();
            services.AddHostedService<HubUIWorker>();

            services.AddSingleton<HubHost>();
            services.AddSingleton<IMessageProcessingBase, MessageProcessing>();
            services.AddSingleton<HubManager>();
            services.AddSingleton<MessageSerializer>();
            services.AddSingleton<MessageMaps>();
            services.AddSingleton<ConnectionManager>();

            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            builder.AddJsonFileFromArgument(args);

            var configuration = builder.Build();

            AppSettings.Register(services, configuration);

            services.AddLogging(builder =>
            {
                builder.AddConfiguration(configuration.GetSection("Logging"));
                builder.AddFile(o => o.RootPath = AppContext.BaseDirectory);
            });

            var assembly = typeof(IMessageHandler).Assembly;

            // Register all handlers.
            assembly.GetTypesImplementing<IMessageHandler>().ForEach((t) =>
            {
                services.AddSingleton(typeof(IMessageHandler), t);
            });

            // Register all events.
            assembly.GetTypesImplementing<IEvent>().ForEach((t) =>
            {
                services.AddSingleton(typeof(IEvent), t);
            });

            // Register all actions.
            assembly.GetTypesImplementing<IAction>().ForEach((t) =>
            {
                services.AddSingleton(typeof(IAction), t);
            });
        });
    }
}
