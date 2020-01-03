using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blockcore.Platform
{
    public class AppSettings
    {
        public OrchestratorOptions Orchestrator { get; private set; }

        public HubOptions Hub { get; private set; }

        public AppSettings(IOptionsMonitor<OrchestratorOptions> orchestrator, IOptionsMonitor<HubOptions> hub)
        {
            Orchestrator = orchestrator.CurrentValue;
            Hub = hub.CurrentValue;

            orchestrator.OnChange((options) => {
                Orchestrator = options;
            });

            hub.OnChange((options) => {
                Hub = options;
            });
        }

        public static void Register(IServiceCollection services, IConfigurationRoot configuration) {

            services.Configure<OrchestratorOptions>(configuration.GetSection("Orchestrator"));
            services.Configure<HubOptions>(configuration.GetSection("Hub"));
            services.AddScoped((sp) => new AppSettings(sp.GetService<IOptionsMonitor<OrchestratorOptions>>(), sp.GetService<IOptionsMonitor<HubOptions>>()));
        }
    }
}
