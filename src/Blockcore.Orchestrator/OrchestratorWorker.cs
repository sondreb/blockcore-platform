using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Blockcore.Orchestrator
{
    public class OrchestratorWorker : BackgroundService
    {
        private readonly ILogger<OrchestratorWorker> log;
        private readonly OrchestratorHost host;

        public OrchestratorWorker(ILogger<OrchestratorWorker> log, OrchestratorHost host)
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
