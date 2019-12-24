using Blockcore.Platform;
using Blockcore.Platform.Networking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;

namespace Blockcore.Gateway
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
                services.AddHostedService<GatewayWorker>();

                services.AddSingleton<GatewayHost>();
                services.AddSingleton<IMessageProcessingBase, MessageProcessing>();
                services.AddSingleton<GatewayManager>();
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
                    //builder.AddFile<FileLoggerProvider>(configure: o => o.RootPath = Path.Combine(AppContext.BaseDirectory, Process.GetCurrentProcess().Id));
                });

                // TODO: This should likely be updated in the future to allow third-party plugin assemblies to be loaded as well.
                // Register all handlers in executing assembly.
                typeof(IMessageGatewayHandler).Assembly.GetTypesImplementing<IMessageGatewayHandler>().ForEach((t) =>
                {
                    services.AddSingleton(typeof(IMessageHandler), t);
                });
            });
    }
}
