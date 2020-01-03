using Blockcore.Platform.Networking.Entities;
using Blockcore.Platform.Networking.Messages;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Blockcore.Platform.Networking
{
    public interface IOrchestratorManager
    {
        ConnectionManager Connections { get; }

        IPEndPoint UdpEndpoint { get; set; } 

        void SendTCP(IBaseEntity entity, NetworkClient client);

        void SendUDP(IBaseEntity entity, IPEndPoint endpoint);

        void BroadcastTCP(IBaseEntity entity);

        void BroadcastUDP(IBaseEntity entity);

        TcpListener Tcp { get; }

        UdpClient Udp { get; }

        void StartTcp();

        void StartUdp();

        void Disconnect(TcpClient Client);

        void ProcessMessage(BaseMessage message, ProtocolType protocol, IPEndPoint endpoint = null, NetworkClient client = null);

        IMessageProcessingBase MessageProcessing { get; set; }
    }
}
