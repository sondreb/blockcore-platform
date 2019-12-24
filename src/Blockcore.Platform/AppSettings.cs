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
        public GatewayOptions Gateway { get; private set; }

        public HubOptions Hub { get; private set; }

        public AppSettings(IOptionsMonitor<GatewayOptions> gateway, IOptionsMonitor<HubOptions> hub)
        {
            Gateway = gateway.CurrentValue;
            Hub = hub.CurrentValue;

            gateway.OnChange((options) => {
                Gateway = options;
            });

            hub.OnChange((options) => {
                Hub = options;
            });
        }

        public static void Register(IServiceCollection services, IConfigurationRoot configuration) {

            services.Configure<GatewayOptions>(configuration.GetSection("Gateway"));
            services.Configure<HubOptions>(configuration.GetSection("Hub"));
            services.AddScoped((sp) => new AppSettings(sp.GetService<IOptionsMonitor<GatewayOptions>>(), sp.GetService<IOptionsMonitor<HubOptions>>()));
        }
    }
}
