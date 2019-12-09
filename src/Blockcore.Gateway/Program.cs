using Blockcore.Platform;
using Blockcore.Platform.Networking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Blockcore.Gateway
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
            services.AddHostedService<GatewayWorker>();

            services.AddSingleton<GatewayHost>();
            services.AddSingleton<IMessageProcessingBase, MessageProcessing>();
            services.AddSingleton<GatewayManager>();
            services.AddSingleton<MessageSerializer>();
            services.AddSingleton<MessageMaps>();
            services.AddSingleton<ConnectionManager>();
            services.AddLogging();

            // TODO: This should likely be updated in the future to allow third-party plugin assemblies to be loaded as well.
            // Register all handlers in executing assembly.
            typeof(IMessageGatewayHandler).Assembly.GetTypesImplementing<IMessageGatewayHandler>().ForEach((t) =>
            {
                services.AddSingleton(typeof(IMessageHandler), t);
            });

        });
    }
}
