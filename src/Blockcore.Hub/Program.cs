using Blockcore.Platform;
using Blockcore.Platform.Networking;
using Blockcore.Platform.Networking.Actions;
using Blockcore.Platform.Networking.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Blockcore.Hub
{
    class Program
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
            services.AddLogging();

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
