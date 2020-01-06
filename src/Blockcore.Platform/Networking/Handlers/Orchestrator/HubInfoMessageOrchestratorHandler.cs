using Blockcore.Platform.Networking.Entities;
using Blockcore.Platform.Networking.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;

namespace Blockcore.Platform.Networking.Handlers
{
    public class HubInfoMessageOrchestratorHandler : IMessageOrchestratorHandler, IHandle<HubInfoMessage>
    {
        private readonly ILogger<HubInfoMessageOrchestratorHandler> log;
        private readonly IOrchestratorManager manager;

        public HubInfoMessageOrchestratorHandler(ILogger<HubInfoMessageOrchestratorHandler> log, IOrchestratorManager manager)
        {
            this.log = log;
            this.manager = manager;
        }

        public void Process(BaseMessage message, ProtocolType protocol, IPEndPoint endpoint = null, NetworkClient networkClient = null)
        {
            var client = networkClient?.TcpClient;
            HubInfo hubInfo = manager.Connections.GetConnection(message.Id);

            if (hubInfo == null)
            {
                hubInfo = new HubInfo((HubInfoMessage)message);
                manager.Connections.AddConnection(hubInfo);

                if (endpoint != null)
                {
                    this.log.LogInformation("Client Added: UDP EP: {0}:{1}, Name: {2}", endpoint.Address, endpoint.Port, hubInfo.Name);
                }
                else if (client != null)
                {
                    if (client.Client.RemoteEndPoint != null)
                    {
                        this.log.LogInformation("Client Added: TCP EP: {0}:{1}, Name: {2}", ((IPEndPoint)client.Client.RemoteEndPoint).Address, ((IPEndPoint)client.Client.RemoteEndPoint).Port, hubInfo.Name);
                    }
                    else
                    {
                        this.log.LogInformation("Client Added: TCP EP:, Name: {0}", hubInfo.Name);
                    }
                }
            }
            else
            {
                hubInfo.Update((HubInfoMessage)message);

                if (endpoint != null)
                { 
                    this.log.LogInformation("Client Updated: UDP EP: {0}:{1}, Name: {2}", endpoint.Address, endpoint.Port, hubInfo.Name);
                }
                else if (client != null)
                {
                    if (client.Client.RemoteEndPoint != null)
                    {
                        this.log.LogInformation("Client Updated: TCP EP: {0}:{1}, Name: {2}", ((IPEndPoint)client.Client.RemoteEndPoint).Address, ((IPEndPoint)client.Client.RemoteEndPoint).Port, hubInfo.Name);
                    }
                    else
                    {
                        this.log.LogInformation("Client Updated: TCP EP:, Name: {0}", hubInfo.Name);
                    }
                }
            }

            if (endpoint != null)
            { 
                hubInfo.ExternalEndpoint = endpoint;
            }

            if (networkClient != null)
            { 
                hubInfo.Client = networkClient;
            }

            // If we have an instance of the TcpClient, make sure we update the hubInfo with it.
            //if (networkClient != null)
            //{
            //    if (hubInfo.Client == null)
            //    {
            //        hubInfo.Client = new NetworkClient(client);
            //    }
            //    else
            //    {
            //        hubInfo.Client.TcpClient = client;
            //    }
            //}

            manager.BroadcastTCP(hubInfo);

            if (!hubInfo.Initialized)
            {
                if (hubInfo.ExternalEndpoint != null & protocol == ProtocolType.Udp)
                    manager.SendUDP(new Chat("Server", hubInfo.Name, "UDP Communication Test", string.Empty), hubInfo.ExternalEndpoint);

                if (hubInfo.Client != null & protocol == ProtocolType.Tcp)
                    manager.SendTCP(new Chat("Server", hubInfo.Name, "TCP Communication Test", string.Empty), hubInfo.Client);

                if (hubInfo.Client != null & hubInfo.ExternalEndpoint != null)
                {
                    foreach (HubInfo ci in manager.Connections.Connections)
                    {
                        manager.SendUDP(ci, hubInfo.ExternalEndpoint);
                    }

                    hubInfo.Initialized = true;
                }
            }
        }
    }
}
