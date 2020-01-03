using Blockcore.Platform;
using Blockcore.Runtime;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NBitcoin;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Blockcore.Hub
{
    public class HubWorker : BackgroundService
    {
        private readonly ILogger<HubWorker> log;
        private readonly HubHost host;
        private readonly AppSettings settings;

        public HubWorker(ILogger<HubWorker> log, HubHost host, AppSettings settings)
        {
            this.log = log;
            this.host = host;
            this.settings = settings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Protection protection = new Protection();
            Mnemonic recoveryPhrase;
            DirectoryInfo dataFolder = new DirectoryInfo(settings.Hub.DataFolder);

            if (!dataFolder.Exists)
            {
                dataFolder.Create();
            }

            var path = Path.Combine(dataFolder.FullName, "recoveryphrase.txt");

            if (!File.Exists(path))
            {
                recoveryPhrase = new Mnemonic(Wordlist.English, WordCount.Twelve);
                var cipher = protection.Protect(recoveryPhrase.ToString());
                File.WriteAllText(path, cipher);
            }
            else
            {
                var cipher = File.ReadAllText(path);
                recoveryPhrase = new Mnemonic(protection.Unprotect(cipher));
            }

            if (recoveryPhrase.ToString() != "border indicate crater public wealth luxury derive media barely survey rule hen")
            {
                //throw new ApplicationException("RECOVERY PHRASE IS DIFFERENT!");
            }

            // Read the identity from the secure storage and provide it here.
            host.Setup(new Identity(recoveryPhrase.ToString()), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                // log.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }

            host.Stop();
        }
    }
}
