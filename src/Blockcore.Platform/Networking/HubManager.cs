using Blockcore.Platform.Networking.Entities;
using Blockcore.Platform.Networking.Events;
using Blockcore.Platform.Networking.Messages;
using Microsoft.Extensions.Logging;
using PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Blockcore.Platform.Networking
{
    public class HubManager : IHubManager
    {
        public IPEndPoint ServerEndpoint { get; private set; }

        public HubInfo LocalHubInfo { get; }

        public List<Ack> AckResponces { get; }

        private IPAddress internetAccessAdapter;
        private TcpClient TCPClient = new TcpClient();
        private UdpClient UDPClient = new UdpClient();
        private Thread ThreadUDPListen;
        private bool _TCPListen = false;

        public bool TCPListen
        {
            get { return _TCPListen; }
            set
            {
                _TCPListen = value;

                if (value)
                {
                    ListenTCP();
                }
            }
        }

        private bool _UDPListen = false;
        public bool UDPListen
        {
            get { return _UDPListen; }
            set
            {
                _UDPListen = value;
                if (value)
                    ListenUDP();
            }
        }

        private readonly ILogger<HubManager> log;
        private readonly MessageSerializer messageSerializer;
        private readonly Hub events;
        private readonly AppSettings options;

        public ConnectionManager Connections { get; }

        public IMessageProcessingBase MessageProcessing { get; set; }

        public HubManager(
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

            UDPClient.AllowNatTraversal(true);
            UDPClient.Client.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
            UDPClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            LocalHubInfo.Name = Environment.MachineName;
            LocalHubInfo.ConnectionType = ConnectionTypes.Unknown;
            LocalHubInfo.Id = DateTime.Now.Ticks.ToString();

            var IPs = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            foreach (var IP in IPs)
            {
                log.LogInformation("Internal Address: {IP}", IP);
                LocalHubInfo.InternalAddresses.Add(IP);
            }
        }

        public void ConnectOrchestrator(string server)
        {
            try
            {
                this.log.LogInformation("Connecting to supplied server: " + server);

                this.ServerEndpoint = IPEndPoint.Parse(server);

                internetAccessAdapter = GetAdapterWithInternetAccess();

                this.log.LogInformation("Adapter with Internet Access: " + internetAccessAdapter);

                TCPClient = new TcpClient();
                TCPClient.Client.Connect(ServerEndpoint);

                UDPListen = true;
                TCPListen = true;

                SendMessageToOrchestratorUDP(LocalHubInfo.Simplified());
                LocalHubInfo.InternalEndpoint = (IPEndPoint)UDPClient.Client.LocalEndPoint;

                Thread.Sleep(550);
                SendMessageToOrchestratorTCP(LocalHubInfo);

                // Every 5 second we'll send a keep alive message to the orchestrator to ensure the connection is
                // kept open and orchestrator knows we've not shut down.
                Thread keepAlive = new Thread(new ThreadStart(delegate
                {
                    var keepAliveMessage = new KeepAlive();

                    while (TCPClient.Connected)
                    {
                        Thread.Sleep(5000);
                        SendMessageToOrchestratorTCP(keepAliveMessage);
                    }
                }));

                keepAlive.IsBackground = true;
                keepAlive.Start();

                events.Publish(new GatewayConnectedEvent() { Self = (HubInfoMessage)LocalHubInfo.ToMessage(), Name = "Gateway" });

            }
            catch (Exception ex)
            {
                this.log.LogError("Error when connecting", ex);
            }
        }

        public void DisconnectOrchestrator(bool disconnectFromHubs)
        {
            UDPListen = !disconnectFromHubs; // If we want to keep hubs open, we'll keep UDP listening on.
            TCPListen = false;

            if (TCPClient.Connected)
            {
                TCPClient.Client.Disconnect(true);
            }

            if (disconnectFromHubs)
            {
                Connections.ClearConnections();
            }

            events.Publish(new OrchestratorDisconnectedEvent());
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

        public void SendMessageToOrchestratorTCP(IBaseEntity entity)
        {
            if (TCPClient != null && TCPClient.Connected)
            {
                byte[] data = messageSerializer.Serialize(entity.ToMessage());

                try
                {
                    NetworkStream NetStream = TCPClient.GetStream();
                    NetStream.Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    this.log.LogError("Error on TCP send", ex);
                }
            }
        }

        /// <inheritdoc />
        public void SendMessageToOrchestratorUDP(IBaseEntity entity)
        {
            var endpoint = this.ServerEndpoint;

            entity.Id = LocalHubInfo.Id;

            byte[] data = messageSerializer.Serialize(entity.ToMessage());

            try
            {
                if (data != null)
                {
                    UDPClient.Send(data, data.Length, endpoint);
                }
            }
            catch (Exception ex)
            {
                this.log.LogError("Error on UDP send", ex);
            }
        }

        /// <inheritdoc />
        public void SendMessageToHubUDP(IBaseEntity entity, IPEndPoint endpoint)
        {
            entity.Id = LocalHubInfo.Id;

            byte[] data = messageSerializer.Serialize(entity.ToMessage());

            try
            {
                if (data != null)
                {
                    UDPClient.Send(data, data.Length, endpoint);
                }
            }
            catch (Exception ex)
            {
                this.log.LogError("Error on UDP send", ex);
            }
        }

        private void ListenUDP()
        {
            ThreadUDPListen = new Thread(new ThreadStart(delegate
            {
                while (UDPListen)
                {
                    try
                    {
                        IPEndPoint endpoint = LocalHubInfo.InternalEndpoint;

                        if (endpoint != null)
                        {
                            byte[] receivedBytes = UDPClient.Receive(ref endpoint);

                            if (receivedBytes != null)
                            {
                                // Retrieve the message from the network stream. This will handle everything from message headers, body and type parsing.
                                var message = messageSerializer.Deserialize(receivedBytes);
                                MessageProcessing.Process(message, ProtocolType.Udp, endpoint);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this.log.LogError("Error on UDP Receive", ex);
                    }
                }
            }));

            ThreadUDPListen.IsBackground = true;

            if (UDPListen)
            {
                ThreadUDPListen.Start();
            }
        }

        private void ListenTCP()
        {
            var tcpListenerThread = new Thread(new ThreadStart(delegate
            {
                var network = new NetworkClient(TCPClient);

                using (var stream = TCPClient.GetStream())
                {
                    while (TCPListen)
                    {
                        try
                        {
                            // Retrieve the message from the network stream. This will handle everything from message headers, body and type parsing.
                            var message = messageSerializer.Deserialize(stream);

                            MessageProcessing.Process(message, ProtocolType.Tcp, null, network);
                        }
                        catch (System.IO.EndOfStreamException endex)
                        {
                            if (!TCPListen)
                            {
                                log.LogInformation("Received EndOfStreamException, but TCPListen was set to false, performing a graceful disconnect.");
                            }
                            else
                            {
                                log.LogError("Received EndOfStreamException while TCPListen was true.", endex);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.LogError("Error on TCP Receive.", ex);
                        }
                    }
                }
            }));

            tcpListenerThread.IsBackground = true;

            if (TCPListen)
            {
                tcpListenerThread.Start();
            }
        }

        public void ConnectToClient(string id)
        {
            var hubInfo = this.Connections.GetConnection(id);
            ConnectToClient(hubInfo);
        }

        public void ConnectToClient(HubInfo hubInfo)
        {
            HubConnectRequest req = new HubConnectRequest(LocalHubInfo.Id, hubInfo.Id);

            // Send a message to the orchestrator, telling the target hub that we would like to connect.
            SendMessageToOrchestratorTCP(req);

            this.log.LogInformation("Sent Connection Request To: " + hubInfo.ToString());

            Thread connect = new Thread(new ThreadStart(delegate
            {
                IPEndPoint responsiveEndpoint = FindReachableEndpoint(hubInfo);

                if (responsiveEndpoint != null)
                {
                    this.log.LogInformation("Connection Successfull to: " + responsiveEndpoint.ToString());

                    events.Publish(new HubConnectionStartedEvent() { Data = (HubInfoMessage)hubInfo.ToMessage(), Endpoint = responsiveEndpoint.ToString() });
                }
            }));

            connect.IsBackground = true;

            connect.Start();
        }

        public IPEndPoint FindReachableEndpoint(HubInfo hubInfo)
        {
            this.log.LogInformation("Attempting to Connect via LAN");

            for (int ip = 0; ip < hubInfo.InternalAddresses.Count; ip++)
            {
                if (!TCPClient.Connected)
                {
                    break;
                }

                IPAddress IP = hubInfo.InternalAddresses[ip];
                IPEndPoint endpoint = new IPEndPoint(IP, hubInfo.InternalEndpoint.Port);

                for (int i = 1; i < 4; i++)
                {
                    if (!TCPClient.Connected)
                    {
                        break;
                    }

                    this.log.LogInformation("Sending Ack to " + endpoint.ToString() + ". Attempt " + i + " of 3");

                    SendMessageToHubUDP(new Ack(LocalHubInfo.Id), endpoint);
                    Thread.Sleep(200);

                    Ack response = AckResponces.FirstOrDefault(a => a.RecipientId == hubInfo.Id);

                    if (response != null)
                    {
                        this.log.LogInformation("Received Ack Responce from " + endpoint.ToString());

                        hubInfo.ConnectionType = ConnectionTypes.LAN;

                        AckResponces.Remove(response);

                        return endpoint;
                    }
                }
            }

            if (hubInfo.ExternalEndpoint != null)
            {
                this.log.LogInformation("Attempting to Connect via Internet");

                for (int i = 1; i < 100; i++)
                {
                    if (!TCPClient.Connected)
                    {
                        break;
                    }

                    this.log.LogInformation("Sending Ack to " + hubInfo.ExternalEndpoint + ". Attempt " + i + " of 99");

                    SendMessageToHubUDP(new Ack(LocalHubInfo.Id), hubInfo.ExternalEndpoint);
                    Thread.Sleep(300);

                    Ack response = AckResponces.FirstOrDefault(a => a.RecipientId == hubInfo.Id);

                    if (response != null)
                    {
                        this.log.LogInformation("Received Ack New from " + hubInfo.ExternalEndpoint.ToString());

                        hubInfo.ConnectionType = ConnectionTypes.WAN;

                        AckResponces.Remove(response);

                        return hubInfo.ExternalEndpoint;
                    }
                }

                this.log.LogInformation("Connection to " + hubInfo.Name + " failed");
            }
            else
            {
                this.log.LogInformation("Client's External EndPoint is Unknown");
            }

            return null;
        }
    }
}
