using Blockcore.Platform.Networking;
using Blockcore.Platform.Networking.Entities;
using Blockcore.Platform.Networking.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Blockcore.Platform.Tests.Fakes
{
    public class FakeOrchestratorManager : IOrchestratorManager
    {
        public ConnectionManager Connections { get; }

        public TcpListener Tcp { get; }

        public UdpClient Udp { get; }

        private ushort port { get { return options.Orchestrator.Port; } }
        private IPEndPoint tcpEndpoint;
        //private TcpListener tcp;
        //private UdpClient udp;
        private readonly ILogger<OrchestratorManager> log;
        private readonly MessageSerializer messageSerializer;
        private readonly AppSettings options;

        public IMessageProcessingBase MessageProcessing { get; set; }

        public IPEndPoint UdpEndpoint { get; set; }

        public FakeOrchestratorManager(
            ILogger<OrchestratorManager> log,
            AppSettings options,
            MessageSerializer messageSerializer,
            ConnectionManager connectionManager)
        {
            this.log = log;
            this.options = options;
            this.messageSerializer = messageSerializer;
            this.Connections = connectionManager;

            tcpEndpoint = new IPEndPoint(IPAddress.Any, port);
            Tcp = new TcpListener(tcpEndpoint);

            UdpEndpoint = new IPEndPoint(IPAddress.Any, port);
            Udp = new UdpClient(UdpEndpoint);

            hubs = new List<IHubManager>();
        }

        private List<IHubManager> hubs;

        // Used to connect outbound messages from orchestrator to the individual hub hosts.
        public void RegisterHubManager(IHubManager hubManager)
        {
            hubs.Add(hubManager);
        }

        public void UnregisterHubManager(IHubManager hubManager)
        {
            hubs.Remove(hubManager);
        }

        public IHubManager GetHubManager(IPEndPoint endpoint)
        {
            var hub = hubs.SingleOrDefault(h => h.ServerEndpoint.Port == endpoint.Port);
            return hub;
        }

        public void BroadcastTCP(IBaseEntity entity)
        {
            //foreach (var hub in hubs)
            //{
            //    SendTCP(entity, null);
            //}

            // Here we must broadcast the same message to all connected hubs.
            foreach (HubInfo hubInfo in Connections.Connections.Where(x => x.Client != null))
            {
                SendTCP(entity, hubInfo.Client);
            }
        }

        public void BroadcastUDP(IBaseEntity entity)
        {
            foreach (HubInfo hubInfo in Connections.Connections)
            {
                SendUDPFake(entity, hubInfo.ExternalEndpoint, hubInfo.Client);
            }
        }

        public void Disconnect(TcpClient Client)
        {
            throw new NotImplementedException();
        }

        public void SendTCP(IBaseEntity entity, NetworkClient client)
        {
            var hub = hubs.SingleOrDefault(hub => hub.LocalHubInfo.InternalAddresses[0].ToString() == client.ClientIP);

            if (hub != null)
            {
                // Simulate serialization.
                //byte[] data = messageSerializer.Serialize(entity.ToMessage());
                //var message = messageSerializer.Deserialize(data);
                
                // Temporarily disable the faking of serialization, due to the orchestrator needing to know the type.

                hub.MessageProcessing.Process(entity.ToMessage(), ProtocolType.Tcp);
            }

            //if (client != null && client.Connected)
            //{
            //    byte[] Data = messageSerializer.Serialize(entity.ToMessage());

            //    NetworkStream NetStream = client.GetStream();
            //    NetStream.Write(Data, 0, Data.Length);
            //}
        }

        public void SendUDP(IBaseEntity entity, IPEndPoint endpoint)
        {
            // For the integration test, we'll rely on the endpoint port to find correct hub.
            var hub = hubs.SingleOrDefault(hub => hub.ServerEndpoint != null && hub.ServerEndpoint.Port == endpoint.Port);

            if (hub != null)
            {
                // Simulate serialization.
                byte[] data = messageSerializer.Serialize(entity.ToMessage());
                var message = messageSerializer.Deserialize(data);

                hub.MessageProcessing.Process(message, ProtocolType.Udp, UdpEndpoint);
            }

            //byte[] Bytes = messageSerializer.Serialize(entity.ToMessage());
            //Udp.Send(Bytes, Bytes.Length, UdpEndpoint);
        }

        public void SendUDPFake(IBaseEntity entity, IPEndPoint endpoint, NetworkClient client)
        {
            var hub = hubs.SingleOrDefault(hub => hub.LocalHubInfo.InternalAddresses[0].ToString() == client.ClientIP);

            if (hub != null)
            {
                // Simulate serialization.
                byte[] data = messageSerializer.Serialize(entity.ToMessage());
                var message = messageSerializer.Deserialize(data);

                hub.MessageProcessing.Process(message, ProtocolType.Udp);
            }
        }

        public void StartTcp()
        {
            throw new NotImplementedException();
        }

        public void StartUdp()
        {
            throw new NotImplementedException();
        }

        public void ProcessMessage(BaseMessage message, ProtocolType protocol, IPEndPoint endpoint = null, NetworkClient client = null)
        {
            MessageProcessing.Process(message, protocol, endpoint, client);
        }
    }
}
