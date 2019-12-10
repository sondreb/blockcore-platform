using Blockcore.Platform.Networking;
using Blockcore.Platform.Networking.Actions;
using Blockcore.Platform.Networking.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace Blockcore.Hub
{
    public class HubHost
    {
        private readonly ILogger<HubHost> log;
        private readonly IMessageProcessingBase messageProcessing;
        private readonly HubManager manager;
        private readonly PubSub.Hub hub = PubSub.Hub.Default;

        public HubHost(
            ILogger<HubHost> log,
            IMessageProcessingBase messageProcessing,
            HubManager manager)
        {
            this.log = log;
            this.messageProcessing = messageProcessing;
            this.manager = manager;

            // Due to circular dependency, we must manually set the MessageProcessing on the Manager.
            this.manager.MessageProcessing = this.messageProcessing;
        }

        public void Launch(CancellationToken token)
        {
            // Prepare the messaging processors for message handling.
            this.messageProcessing.Build();

            hub.Subscribe<ConnectionAddedEvent>(this, e =>
            {
                Console.WriteLine($"ConnectionAddedEvent: {e.Data.Id}");
                Console.WriteLine($"                    : ExternalIPAddress: {e.Data.ExternalEndpoint}");
                Console.WriteLine($"                    : InternalIPAddress: {e.Data.InternalEndpoint}");
                Console.WriteLine($"                    : Name: {e.Data.Name}");

                foreach (var address in e.Data.InternalAddresses)
                {
                    Console.WriteLine($"                    : Address: {address}");
                }
            });

            hub.Subscribe<ConnectionRemovedEvent>(this, e =>
            {
                Console.WriteLine($"ConnectionRemovedEvent: {e.Data.Id}");
            });

            hub.Subscribe<ConnectionStartedEvent>(this, e =>
            {
                Console.WriteLine($"ConnectionStartedEvent: {e.Endpoint}");
            });

            hub.Subscribe<ConnectionStartingEvent>(this, e =>
            {
                Console.WriteLine($"ConnectionStartedEvent: {e.Data.Id}");
            });

            hub.Subscribe<ConnectionUpdatedEvent>(this, e =>
            {
                Console.WriteLine($"ConnectionUpdatedEvent: {e.Data.Id}");
            });

            hub.Subscribe<GatewayConnectedEvent>(this, e =>
            {
                Console.WriteLine("Connected to Gateway");
            });

            hub.Subscribe<GatewayShutdownEvent>(this, e =>
            {
                Console.WriteLine("Disconnected from Gateway");
            });

            hub.Subscribe<HubInfoEvent>(this, e =>
            {
            });

            hub.Subscribe<MessageReceivedEvent>(this, e =>
            {
                Console.WriteLine($"MessageReceivedEvent: {e.Data.Content}");
            });

            hub.Subscribe<ConnectGatewayAction>(this, e =>
            {
                this.manager.ConnectGateway(e.Server);
            });

            hub.Subscribe<DisconnectGatewayAction>(this, e =>
            {
                this.manager.DisconnectGateway();
            });

            hub.Subscribe<ConnectHubAction>(this, e =>
            {
                this.manager.ConnectToClient(e.Id);
            });
        }

        public void Stop()
        {
            this.manager.DisconnectGateway();
        }
    }
}
