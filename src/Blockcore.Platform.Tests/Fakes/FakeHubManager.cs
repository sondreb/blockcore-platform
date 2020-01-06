using Blockcore.Platform.Networking;
using Blockcore.Platform.Networking.Entities;
using Blockcore.Platform.Networking.Events;
using Blockcore.Platform.Networking.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Blockcore.Platform.Tests.Fakes
{
    public class FakeHubManager : IHubManager
    {
        public IMessageProcessingBase MessageProcessing { get; set; }

        public IPEndPoint ServerEndpoint { get; private set; }

        public HubInfo LocalHubInfo { get; }

        public List<Ack> AckResponces { get; }

        public bool TCPListen { get; set; }
        public bool UDPListen { get; set; }

        public ConnectionManager Connections { get; }

        private readonly PubSub.Hub events;
        private readonly ILogger<HubManager> log;
        private readonly MessageSerializer messageSerializer;
        private readonly AppSettings options;
        private IPAddress internetAccessAdapter;
        private IOrchestratorManager orchestratorManager;

        private TcpClient TCPClient = new TcpClient();
        private UdpClient UDPClient = new UdpClient();

        public FakeHubManager(
            ILogger<HubManager> log,
            PubSub.Hub events,
            AppSettings options,
            ConnectionManager connectionManager,
            MessageSerializer messageSerializer)
        {
            this.log = log;
            this.events = events;
            this.options = options;
            this.messageSerializer = messageSerializer;
            this.Connections = connectionManager;

            this.LocalHubInfo = new HubInfo();

            this.AckResponces = new List<Ack>();

            LocalHubInfo.Name = Environment.MachineName;
            LocalHubInfo.ConnectionType = ConnectionTypes.Unknown;

            //LocalHubInfo.Id = DateTime.Now.Ticks.ToString();


            //var IPs = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            var IPs = new IPAddress[1] { GetRandomPrivateIpAddress() };

            foreach (var IP in IPs)
            {
                log.LogInformation("Internal Address: {IP}", IP);
                LocalHubInfo.InternalAddresses.Add(IP);
            }
        }

        public IPAddress GetRandomPrivateIpAddress()
        {
            var random = new Random();
            var ip = $"10.0.{random.Next(0, 255)}.{random.Next(0, 255)}";
            return IPAddress.Parse(ip);
        }

        public IPAddress GetRandomPublicIpAddress()
        {
            var random = new Random();
            var ip = $"{random.Next(1, 255)}.{random.Next(0, 255)}.{random.Next(0, 255)}.{random.Next(0, 255)}";
            return IPAddress.Parse(ip);
        }

        /// <summary>
        /// This is how we fake the interaction with the orchestrator.
        /// </summary>
        /// <param name="orchestratorManager"></param>
        public void SetOrchestrator(IOrchestratorManager orchestratorManager)
        {
            this.orchestratorManager = orchestratorManager;
        }

        public void ConnectOrchestrator(string server)
        {
            this.ServerEndpoint = IPEndPoint.Parse(server);

            internetAccessAdapter = GetAdapterWithInternetAccess();

            // We must put our local IP on the TcpClient, which the orchestrator will use to find this instance of hub.
            //            var endpoint = new IPEndPoint(LocalHubInfo.InternalAddresses[0], 6610);

            //TCPClient = new TcpClient(LocalHubInfo.InternalAddresses[0].ToString(), 6610);
            TCPClient = new TcpClient();
            //TCPClient.Client.Connect(ServerEndpoint);

            UDPListen = true;
            TCPListen = true;

            SendMessageToOrchestratorUDP(LocalHubInfo.Simplified());
            LocalHubInfo.InternalEndpoint = (IPEndPoint)UDPClient.Client.LocalEndPoint;

            SendMessageToOrchestratorTCP(LocalHubInfo);

            SendMessageToOrchestratorTCP(new KeepAlive());

            // This is the normal event that is triggered in the full implementation after connecting.
            events.Publish(new GatewayConnectedEvent() { Self = (HubInfoMessage)LocalHubInfo.ToMessage(), Name = "Gateway" });
        }

        private IPAddress GetAdapterWithInternetAccess()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_IP4RouteTable WHERE Destination=\"0.0.0.0\"");

            int interfaceIndex = -1;

            foreach (var item in searcher.Get())
            {
                interfaceIndex = Convert.ToInt32(item["InterfaceIndex"]);
            }

            searcher = new ManagementObjectSearcher("root\\CIMV2", string.Format("SELECT * FROM Win32_NetworkAdapterConfiguration WHERE InterfaceIndex={0}", interfaceIndex));

            foreach (var item in searcher.Get())
            {
                string[] IPAddresses = (string[])item["IPAddress"];

                foreach (string IP in IPAddresses)
                {
                    return IPAddress.Parse(IP);
                }
            }

            return null;
        }

        public void ConnectToClient(string id)
        {
            var hubInfo = this.Connections.GetConnection(id);
            ConnectToClient(hubInfo);
        }

        public void ConnectToClient(HubInfo hubInfo)
        {
            HubConnectRequest req = new HubConnectRequest(LocalHubInfo.Id, hubInfo.Id);

            SendMessageToOrchestratorTCP(req);

            this.log.LogInformation("Sent Connection Request To: " + hubInfo.ToString());

            IPEndPoint responsiveEndpoint = FindReachableEndpoint(hubInfo);

            if (responsiveEndpoint != null)
            {
                this.log.LogInformation("Connection Successfull to: " + responsiveEndpoint.ToString());

                events.Publish(new HubConnectionStartedEvent() { 
                    OriginId = hubInfo.Id,
                    TargetId = LocalHubInfo.Id,
                    Data = (HubInfoMessage)hubInfo.ToMessage(), 
                    Endpoint = responsiveEndpoint.ToString() });
            }

            //Thread connect = new Thread(new ThreadStart(delegate
            //{
            //    IPEndPoint responsiveEndpoint = FindReachableEndpoint(hubInfo);

            //    if (responsiveEndpoint != null)
            //    {
            //        this.log.LogInformation("Connection Successfull to: " + responsiveEndpoint.ToString());

            //        events.Publish(new ConnectionStartedEvent() { Data = (HubInfoMessage)hubInfo.ToMessage(), Endpoint = responsiveEndpoint.ToString() });
            //    }
            //}));

            //connect.IsBackground = true;

            //connect.Start();
        }

        public void DisconnectOrchestrator(bool disconnectFromHubs)
        {
            throw new NotImplementedException();
        }

        public IPEndPoint FindReachableEndpoint(HubInfo hubInfo)
        {
            this.log.LogInformation("Attempting to Connect via LAN");

            var endpoint = hubInfo.ExternalEndpoint;

            FakeOrchestratorManager orchestrator = (FakeOrchestratorManager)this.orchestratorManager;
            var hub = orchestrator.GetHubManager(endpoint);

            // First send out the Ack message - then wait for the response.
            // The endpoint should be "us", where the return message should go, which is our "ServerEndpoint".
            hub.MessageProcessing.Process(new Ack(LocalHubInfo.Id).ToMessage(), ProtocolType.Udp, ServerEndpoint);

            Thread.Yield();
            Thread.Sleep(500);
            Thread.Yield();

            Ack response = AckResponces.FirstOrDefault(a => a.RecipientId == hubInfo.Id);

            if (response != null)
            {
                this.log.LogInformation("Received Ack Responce from " + endpoint.ToString());

                hubInfo.ConnectionType = ConnectionTypes.LAN;

                AckResponces.Remove(response);

                return endpoint;
            }

            return null;

            //for (int ip = 0; ip < hubInfo.InternalAddresses.Count; ip++)
            //{
            //    if (!TCPClient.Connected)
            //    {
            //        break;
            //    }

            //    IPAddress IP = hubInfo.InternalAddresses[ip];
            //    IPEndPoint endpoint = new IPEndPoint(IP, hubInfo.InternalEndpoint.Port);

            //    for (int i = 1; i < 4; i++)
            //    {
            //        if (!TCPClient.Connected)
            //        {
            //            break;
            //        }

            //        this.log.LogInformation("Sending Ack to " + endpoint.ToString() + ". Attempt " + i + " of 3");

            //        SendMessageUDP(new Ack(LocalHubInfo.Id), endpoint);
            //        Thread.Sleep(200);

            //        Ack response = AckResponces.FirstOrDefault(a => a.RecipientId == hubInfo.Id);

            //        if (response != null)
            //        {
            //            this.log.LogInformation("Received Ack Responce from " + endpoint.ToString());

            //            hubInfo.ConnectionType = ConnectionTypes.LAN;

            //            AckResponces.Remove(response);

            //            return endpoint;
            //        }
            //    }
            //}

            //if (hubInfo.ExternalEndpoint != null)
            //{
            //    this.log.LogInformation("Attempting to Connect via Internet");

            //    for (int i = 1; i < 100; i++)
            //    {
            //        if (!TCPClient.Connected)
            //        {
            //            break;
            //        }

            //        this.log.LogInformation("Sending Ack to " + hubInfo.ExternalEndpoint + ". Attempt " + i + " of 99");

            //        SendMessageUDP(new Ack(LocalHubInfo.Id), hubInfo.ExternalEndpoint);
            //        Thread.Sleep(300);

            //        Ack response = AckResponces.FirstOrDefault(a => a.RecipientId == hubInfo.Id);

            //        if (response != null)
            //        {
            //            this.log.LogInformation("Received Ack New from " + hubInfo.ExternalEndpoint.ToString());

            //            hubInfo.ConnectionType = ConnectionTypes.WAN;

            //            AckResponces.Remove(response);

            //            return hubInfo.ExternalEndpoint;
            //        }
            //    }

            //    this.log.LogInformation("Connection to " + hubInfo.Name + " failed");
            //}
            //else
            //{
            //    this.log.LogInformation("Client's External EndPoint is Unknown");
            //}

            //return null;
        }

        public void SendMessageToOrchestratorTCP(IBaseEntity entity)
        {
            // Simulate serialization.
            byte[] data = messageSerializer.Serialize(entity.ToMessage());
            var message = messageSerializer.Deserialize(data);

            // Send the message to the fake orchestrator.
            // TODO: We can't recreate the NetworkClient all the time like this?
            orchestratorManager.ProcessMessage(entity.ToMessage(), ProtocolType.Tcp, null, new NetworkClient(TCPClient, LocalHubInfo.InternalAddresses[0].ToString()));
        }

        public void SendMessageToOrchestratorUDP(IBaseEntity entity)
        {
            // Simulate serialization.
            byte[] data = messageSerializer.Serialize(entity.ToMessage());
            var message = messageSerializer.Deserialize(data);

            var endpoint = this.ServerEndpoint;
            orchestratorManager.ProcessMessage(entity.ToMessage(), ProtocolType.Udp, endpoint, new NetworkClient(TCPClient, LocalHubInfo.InternalAddresses[0].ToString()));
        }

        public void SendMessageToHubUDP(IBaseEntity entity, IPEndPoint endpoint)
        {
            if (endpoint == null)
            {
                return;
            }

            FakeOrchestratorManager orchestrator = (FakeOrchestratorManager)orchestratorManager;
            var hub = orchestrator.GetHubManager(endpoint);

            if (hub != null)
            {
                // Simulate serialization.
                byte[] data = messageSerializer.Serialize(entity.ToMessage());
                var message = messageSerializer.Deserialize(data);

                hub.MessageProcessing.Process(message, ProtocolType.Udp, endpoint, new NetworkClient(TCPClient, LocalHubInfo.InternalAddresses[0].ToString()));
            }
        }
    }
}
