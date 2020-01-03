using Blockcore.Platform.Networking.Entities;
using Blockcore.Platform.Networking.Messages;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Blockcore.Platform.Networking
{
    public class OrchestratorManager : IOrchestratorManager
    {
        private ushort port { get { return options.Orchestrator.Port; } }
        private IPEndPoint tcpEndpoint;
        private TcpListener tcp;
        public IPEndPoint UdpEndpoint { get; set; }
        private UdpClient udp;
        private readonly ILogger<OrchestratorManager> log;
        private readonly MessageSerializer messageSerializer;
        private readonly AppSettings options;

        public ConnectionManager Connections { get; }

        public IMessageProcessingBase MessageProcessing { get; set; }

        public OrchestratorManager(
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
            tcp = new TcpListener(tcpEndpoint);

            UdpEndpoint = new IPEndPoint(IPAddress.Any, port);
            udp = new UdpClient(UdpEndpoint);
        }

        public void SendTCP(IBaseEntity entity, NetworkClient client)
        {
            if (client.TcpClient != null && client.TcpClient.Connected)
            {
                byte[] Data = messageSerializer.Serialize(entity.ToMessage());

                NetworkStream NetStream = client.TcpClient.GetStream();
                NetStream.Write(Data, 0, Data.Length);
            }
        }

        public void SendUDP(IBaseEntity entity, IPEndPoint endpoint)
        {
            byte[] Bytes = messageSerializer.Serialize(entity.ToMessage());

            udp.Send(Bytes, Bytes.Length, UdpEndpoint);
        }

        public void BroadcastTCP(IBaseEntity entity)
        {
            foreach (HubInfo hubInfo in Connections.Connections.Where(x => x.Client != null))
            { 
                SendTCP(entity, hubInfo.Client);
            }
        }

        public void BroadcastUDP(IBaseEntity entity)
        {
            foreach (HubInfo hubInfo in Connections.Connections)
            { 
                SendUDP(entity, hubInfo.ExternalEndpoint);
            }
        }

        public TcpListener Tcp { get { return tcp; } }

        public UdpClient Udp { get { return udp; } }

        //public IPEndPoint UdpEndpoint { get { return udpEndpoint; } set { udpEndpoint = value; } }

        public void StartTcp()
        {
            tcp.Start();

            log.LogInformation($"TCP listener started on port {port}.");
        }

        public void StartUdp()
        {

        }

        public void ProcessMessage(BaseMessage message, ProtocolType protocol, IPEndPoint endpoint = null, NetworkClient client = null)
        {
            this.MessageProcessing.Process(message, protocol, endpoint, client);
        }

        public void Disconnect(TcpClient Client)
        {
            HubInfo CI = Connections.Connections.FirstOrDefault(x => x.Client?.TcpClient == Client);

            if (CI != null)
            {
                Connections.RemoveConnection(CI);

                log.LogInformation($"Client disconnected {Client.Client.RemoteEndPoint}");

                Client.Close();

                BroadcastTCP(new Notification(NotificationsTypes.Disconnected, CI.Id));
            }
        }
    }
}
