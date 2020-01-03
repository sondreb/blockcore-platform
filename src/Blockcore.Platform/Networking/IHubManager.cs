using Blockcore.Platform.Networking.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Blockcore.Platform.Networking
{
    public interface IHubManager
    {
        IMessageProcessingBase MessageProcessing { get; set; }

        void ConnectGateway(string server);

        void DisconnectGateway();

        void SendMessageToOrchestratorTCP(IBaseEntity entity);

        void SendMessageToOrchestratorUDP(IBaseEntity entity);

        /// <summary>
        /// Sends a message to a specified hub. The message is sent over UDP.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="endpoint"></param>
        void SendMessageToHubUDP(IBaseEntity entity, IPEndPoint endpoint);

        void ConnectToClient(string id);

        void ConnectToClient(HubInfo hubInfo);

        IPEndPoint FindReachableEndpoint(HubInfo hubInfo);

        public IPEndPoint ServerEndpoint { get; }

        public HubInfo LocalHubInfo { get; }

        public List<Ack> AckResponces { get; }

        public bool TCPListen { get; set; }

        public bool UDPListen { get; set; }

        ConnectionManager Connections { get; }
    }
}
