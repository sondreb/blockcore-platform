using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Blockcore.Gateway
{
    public class GatewayWorker : BackgroundService
    {
        private readonly ILogger<GatewayWorker> _logger;
        private readonly GatewayHost host;

        public GatewayWorker(ILogger<GatewayWorker> logger, GatewayHost host)
        {
            _logger = logger;
            this.host = host;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            host.Launch(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                // _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }

            host.Stop();
        }
    }
}
