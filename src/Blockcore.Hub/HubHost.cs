using Blockcore.Platform;
using Blockcore.Platform.Networking;
using Blockcore.Platform.Networking.Actions;
using Blockcore.Platform.Networking.Entities;
using Blockcore.Platform.Networking.Events;
using Blockcore.Platform.Networking.Exceptions;
using Microsoft.Extensions.Logging;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Blockcore.Hub
{
    public class HubHost
    {
        private readonly ILogger<HubHost> log;
        private readonly IMessageProcessingBase messageProcessing;
        private readonly IHubManager manager;
        private readonly PubSub.Hub events;

        public IHubManager Manager { get { return manager; } }

        public Dictionary<string, HubInfo> AvailableHubs { get; }

        public Dictionary<string, HubInfo> ConnectedHubs { get; }

        public Identity Identity { get; private set; }

        public HubHost(
            ILogger<HubHost> log,
            PubSub.Hub events,
            IMessageProcessingBase messageProcessing,
            IHubManager manager)
        {
            this.log = log;
            this.events = events;
            this.messageProcessing = messageProcessing;
            this.manager = manager;

            // Due to circular dependency, we must manually set the MessageProcessing on the Manager.
            this.manager.MessageProcessing = this.messageProcessing;

            
            AvailableHubs = new Dictionary<string, HubInfo>();
            ConnectedHubs = new Dictionary<string, HubInfo>();
        }

        public void Setup(Identity identity, CancellationToken token)
        {
            // Make the identity availble on the HubHost.
            this.Identity = identity;

            // Appears we also must have the identity in the HubManager.
            this.manager.Identity = identity;

            // Put the public key on the Id of the HubInfo instance.
            this.manager.LocalHubInfo.Id = Identity.Id;

            // Prepare the messaging processors for message handling.
            this.messageProcessing.Build();

            events.Subscribe<HubRegisteredEvent>(this, e =>
            {
                if (e.Data.Id == Identity.Id)
                {
                    return;
                }

                AvailableHubs.Add(e.Data.Id, new HubInfo(e.Data));
            });

            events.Subscribe<HubUpdatedEvent>(this, e =>
            {
                if (e.Data.Id == Identity.Id)
                {
                    return;
                }

                if (AvailableHubs.ContainsKey(e.Data.Id))
                {
                    AvailableHubs[e.Data.Id] = new HubInfo(e.Data);
                }
                else
                {
                    AvailableHubs.Add(e.Data.Id, new HubInfo(e.Data));
                }
            });

            events.Subscribe<HubUnregisteredEvent>(this, e =>
            {
                if (e.Data.Id == Identity.Id)
                {
                    return;
                }

                AvailableHubs.Remove(e.Data.Id);
            });

            events.Subscribe<ConnectGatewayAction>(this, e =>
            {
                this.Connect(e.Server);
            });

            events.Subscribe<ConnectionAddedEvent>(this, e =>
            {
                log.LogInformation($"ConnectionAddedEvent: {e.Data.Id}");
                log.LogInformation($"                    : ExternalIPAddress: {e.Data.ExternalEndpoint}");
                log.LogInformation($"                    : InternalIPAddress: {e.Data.InternalEndpoint}");
                log.LogInformation($"                    : Name: {e.Data.Name}");

                foreach (var address in e.Data.InternalAddresses)
                {
                    log.LogInformation($"                    : Address: {address}");
                }
            });

            events.Subscribe<ConnectionRemovedEvent>(this, e =>
            {
                log.LogInformation($"ConnectionRemovedEvent: {e.Data.Id}");

                if (ConnectedHubs.ContainsKey(e.Data.Id))
                {
                    ConnectedHubs.Remove(e.Data.Id);
                }
            });

            events.Subscribe<HubConnectionStartedEvent>(this, e =>
            {
                log.LogInformation("ConnectionStartedEvent on Endpoint: {Endpoint} and ExternalEndpoint: {ExternalEndpoint} between Self: {Id} and {ExternalId}.", e.Endpoint, e.Data.ExternalEndpoint, Identity.Id, e.Data.Id);

                // Find the connecting HubInfo from the AvailableHubs, based on the event parameter. The HubInfo (Data property) available on the event is our own.
                var originHub = this.AvailableHubs[e.OriginId];
                
                ConnectedHubs.Add(e.OriginId, originHub);

            });

            events.Subscribe<ConnectionStartingEvent>(this, e =>
            {
                log.LogInformation($"ConnectionStartedEvent: {e.Data.Id}");
            });

            events.Subscribe<ConnectionUpdatedEvent>(this, e =>
            {
                log.LogInformation($"ConnectionUpdatedEvent: {e.Data.Id}");
            });

            events.Subscribe<GatewayConnectedEvent>(this, e =>
            {
                log.LogInformation("Connected to Gateway");
            });

            events.Subscribe<OrchestratorShutdownEvent>(this, e =>
            {
                log.LogInformation("Disconnected from Gateway");
            });

            events.Subscribe<HubInfoEvent>(this, e =>
            {
            });

            events.Subscribe<MessageReceivedEvent>(this, e =>
            {
                log.LogInformation("MessageReceivedEvent: From: {From} - To: {To} - Content: {Content} - From ID {Id} - Target ID {TargetId}.", 
                    e.Data.From, 
                    e.Data.To, 
                    e.Data.Content, 
                    e.Data.Id,
                    e.Data.RecipientId);
            });

            events.Subscribe<DisconnectGatewayAction>(this, e =>
            {
                this.manager.DisconnectOrchestrator(e.DisconnectHubs);
            });

            events.Subscribe<ConnectHubAction>(this, e =>
            {
                this.manager.ConnectToClient(e.Id);
            });
        }

        /// <summary>
        /// Connects to the gateway. Can also be achieve by raising the <see cref="ConnectGatewayAction"/> action.
        /// </summary>
        public void Connect(string server)
        {
            manager.ConnectOrchestrator(server);
        }

        public void Stop()
        {
            this.manager.DisconnectOrchestrator(true);
        }

        public void ConnectToHub(string id)
        {
            this.manager.ConnectToClient(id);
        }

        public void InitiateHubConnection(string id)
        {
            var hubInfo = this.AvailableHubs[id];

            if (hubInfo == null)
            {
                throw new PlatformException($"Unable to find the hub to initiate a connection with. Specified ID is {id}.");
            }

            HubHandshake msg = new HubHandshake(Identity.Id, hubInfo.Id);

            // TODO: Here we must construct the full handshake request, and sign that message.
            // We must also share an symmetric encryption key to be used for future message exchange.

            // Restore the public key from the shared value.
            
            
            var publicKey = new PubKey(hubInfo.Id);
            var payload = "hello";
            var cipher = publicKey.Encrypt(payload); // Encrypt the request with the public key of the receiver.
            msg.Payload = cipher;

            manager.SendMessageToOrchestratorTCP(msg);
        }

        public void SendMessageToHub(IBaseEntity entity, HubInfo hub)
        {
            if (string.IsNullOrWhiteSpace(entity.Id))
            { 
                entity.Id = this.manager.LocalHubInfo.Id;
            }

            this.manager.SendMessageToHubUDP(entity, hub.ExternalEndpoint);
        }

        public void SendMessageToHub(IBaseEntity entity, string id)
        {
            var hub = ConnectedHubs[id];
            SendMessageToHub(entity, hub);
        }
    }
}
