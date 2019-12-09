using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Blockcore.Hub
{
    public class HubWorker : BackgroundService
    {
        private readonly ILogger<HubWorker> log;
        private readonly HubHost host;

        public HubWorker(ILogger<HubWorker> log, HubHost host)
        {
            this.log = log;
            this.host = host;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            host.Launch(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                // log.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }

            host.Stop();
        }
    }
}
