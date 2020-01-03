using Blockcore.Platform.Networking.Entities;
using Blockcore.Platform.Networking.Events;
using Blockcore.Platform.Networking.Messages;
using Microsoft.Extensions.Logging;
using PubSub;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Blockcore.Platform.Networking.Handlers
{
    public class AckMessageHandler : IMessageHandler, IHandle<AckMessage>
    {
        private readonly Hub events;
        private readonly ILogger<AckMessageHandler> log;
        private readonly IHubManager manager;

        public AckMessageHandler(ILogger<AckMessageHandler> log, PubSub.Hub events, IHubManager manager)
        {
            this.log = log;
            this.events = events;
            this.manager = manager;
        }

        public void Process(BaseMessage message, ProtocolType protocol, IPEndPoint endpoint = null, NetworkClient networkClient = null)
        {
            var client = networkClient?.TcpClient;
            AckMessage msg = (AckMessage)message;

            if (msg.Response)
            {
                manager.AckResponces.Add(new Ack(msg));
            }
            else
            {
                var CI = manager.Connections.GetConnection(msg.Id);

                if (CI.ExternalEndpoint.Address.Equals(endpoint.Address) & CI.ExternalEndpoint.Port != endpoint.Port)
                {
                    this.log.LogInformation("Received Ack on Different Port (" + endpoint.Port + "). Updating ...");

                    CI.ExternalEndpoint.Port = endpoint.Port;

                    events.Publish(new ConnectionUpdatedEvent() { Data = (HubInfoMessage)CI.ToMessage() });
                }

                List<string> IPs = new List<string>();
                CI.InternalAddresses.ForEach(new Action<IPAddress>(delegate (IPAddress IP) { IPs.Add(IP.ToString()); }));

                if (!CI.ExternalEndpoint.Address.Equals(endpoint.Address) & !IPs.Contains(endpoint.Address.ToString()))
                {
                    this.log.LogInformation("Received Ack on New Address (" + endpoint.Address + "). Updating ...");

                    CI.InternalAddresses.Add(endpoint.Address);
                }

                msg.Response = true;
                msg.RecipientId = manager.LocalHubInfo.Id;
                manager.SendMessageToHubUDP(new Ack(msg), endpoint);
            }
        }
    }
}
