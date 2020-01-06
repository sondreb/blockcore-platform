using Blockcore.Platform;
using Blockcore.Platform.Networking;
using Blockcore.Platform.Networking.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Blockcore.Orchestrator
{
    public class OrchestratorHost
    {
        private readonly ILogger<OrchestratorHost> log;
        private readonly IMessageProcessingBase messageProcessing;
        private readonly MessageSerializer messageSerializer;
        private readonly IOrchestratorManager manager;
        private readonly AppSettings options;

        public OrchestratorHost(
            ILogger<OrchestratorHost> log,
            AppSettings options,
            IMessageProcessingBase messageProcessing,
            MessageSerializer messageSerializer,
            IOrchestratorManager connectionManager)
        {
            this.log = log;
            this.options = options;
            this.messageProcessing = messageProcessing;
            this.messageSerializer = messageSerializer;
            this.manager = connectionManager;

            // Due to circular dependency, we must manually set the MessageProcessing on the Manager.
            this.manager.MessageProcessing = this.messageProcessing;
        }

        public void Setup()
        {
            // Prepare the messaging processors for message handling.
            this.messageProcessing.Build();
        }

        public void Launch(CancellationToken token)
        {
            Setup();

            Task tcpTask = Task.Run(() => {
                TcpWorkerAsync(token);
            }, token);

            Task udTask = Task.Run(() => {
                UdpWorker(token);
            }, token);
        }

        public void Stop()
        {
            // We will broadcast a shutdown when we're stopping.
            manager.BroadcastTCP(new Notification(NotificationsTypes.ServerShutdown, null));
        }

        private async Task TcpWorkerAsync(CancellationToken token)
        {
            manager.StartTcp();

            this.log.LogDebug("Accepting incoming connections.");

            while (!token.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await this.manager.Tcp.AcceptTcpClientAsync().WithCancellationAsync(token).ConfigureAwait(false);

                    Task tcpTask = Task.Run(() => {
                        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                        var network = new NetworkClient(client);

                        using (var stream = client.GetStream())
                        {
                            while (client.Connected)
                            {
                                try
                                {
                                    // Retrieve the message from the network stream. This will handle everything from message headers, body and type parsing.
                                    var message = messageSerializer.Deserialize(stream);

                                    this.manager.ProcessMessage(message, ProtocolType.Tcp, null, network);
                                }
                                catch (Exception ex)
                                {
                                    this.log.LogError(ex, "Failed to process incoming message.");
                                    manager.Disconnect(client);
                                    return;
                                }
                            }
                        }

                        manager.Disconnect(client);
                    }, token);

                    //Action<object> processData = new Action<object>(delegate (object tcp)
                    //{

                    //});

                    //Thread threadProcessData = new Thread(new ParameterizedThreadStart(processData));
                    //threadProcessData.Start(tcpClient);
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
            log.LogInformation($"UDP listener started on port {manager.UdpEndpoint.Port}.");

            while (!token.IsCancellationRequested)
            {
                try
                {
                    //var endpoint = manager.UdpEndpoint;
                    IPEndPoint endpoint = null;

                    byte[] receivedBytes = manager.Udp.Receive(ref endpoint);

                    manager.UdpEndpoint = endpoint;

                    if (receivedBytes != null)
                    {
                        // Retrieve the message from the network stream. This will handle everything from message headers, body and type parsing.
                        var message = messageSerializer.Deserialize(receivedBytes);
                        messageProcessing.Process(message, ProtocolType.Udp, manager.UdpEndpoint);
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
