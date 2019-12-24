using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Blockcore.Gateway
{
    public class GatewayWorker : BackgroundService
    {
        private readonly ILogger<GatewayWorker> log;
        private readonly GatewayHost host;

        public GatewayWorker(ILogger<GatewayWorker> log, GatewayHost host)
        {
            this.log = log;
            this.host = host;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            host.Launch(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                log.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }

            host.Stop();
        }
    }
}
