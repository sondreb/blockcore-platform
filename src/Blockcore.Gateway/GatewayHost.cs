using Blockcore.Platform;
using Blockcore.Platform.Networking;
using Blockcore.Platform.Networking.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Blockcore.Gateway
{
    public class GatewayHost
    {
        private readonly ILogger<GatewayHost> log;
        private readonly IMessageProcessingBase messageProcessing;
        private readonly MessageSerializer messageSerializer;
        private readonly GatewayManager connectionManager;
        private readonly AppSettings options;

        public GatewayHost(
            ILogger<GatewayHost> log,
            AppSettings options,
            IMessageProcessingBase messageProcessing,
            MessageSerializer messageSerializer,
            GatewayManager connectionManager)
        {
            this.log = log;
            this.options = options;
            this.messageProcessing = messageProcessing;
            this.messageSerializer = messageSerializer;
            this.connectionManager = connectionManager;
        }

        public void Launch(CancellationToken token)
        {
            // Prepare the messaging processors for message handling.
            this.messageProcessing.Build();

            Task tcpTask = Task.Run(() => {
                TcpWorker(token);
            }, token);

            Task udTask = Task.Run(() => {
                UdpWorker(token);
            }, token);
        }

        public void Stop()
        {
            // We will broadcast a shutdown when we're stopping.
            connectionManager.BroadcastTCP(new Notification(NotificationsTypes.ServerShutdown, null));
        }

        private void TcpWorker(CancellationToken token)
        {
            connectionManager.StartTcp();

            while (!token.IsCancellationRequested)
            {
                try
                {
                    TcpClient newClient = connectionManager.Tcp.AcceptTcpClient();

                    Action<object> processData = new Action<object>(delegate (object tcp)
                    {
                        TcpClient client = (TcpClient)tcp;
                        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                        while (client.Connected)
                        {
                            try
                            {
                                // Retrieve the message from the network stream. This will handle everything from message headers, body and type parsing.
                                var message = messageSerializer.Deserialize(client.GetStream());
                                
                                messageProcessing.Process(message, ProtocolType.Tcp, null, client);
                            }
                            catch (Exception ex)
                            {
                                this.log.LogError(ex, "Failed to process incoming message.");
                                connectionManager.Disconnect(client);
                            }
                        }

                        connectionManager.Disconnect(client);
                    });

                    Thread threadProcessData = new Thread(new ParameterizedThreadStart(processData));
                    threadProcessData.Start(newClient);
                }
                catch (Exception ex)
                {
                    this.log.LogError(ex, "TCP error");

                    // We'll sleep a short while before connecting, to avoid extensive resource usage.
                    Thread.Sleep(250);
                }
            }
        }

        private void UdpWorker(CancellationToken token)
        {
            log.LogInformation($"UDP listener started on port {connectionManager.udpEndpoint.Port}.");

            while (!token.IsCancellationRequested)
            {
                try
                {
                    byte[] receivedBytes = connectionManager.Udp.Receive(ref connectionManager.udpEndpoint);

                    if (receivedBytes != null)
                    {
                        // Retrieve the message from the network stream. This will handle everything from message headers, body and type parsing.
                        var message = messageSerializer.Deserialize(receivedBytes);
                        messageProcessing.Process(message, ProtocolType.Udp, connectionManager.udpEndpoint);
                    }
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "UDP error");
                }
            }
        }
    }
}
