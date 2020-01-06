using Blockcore.Platform.Networking.Messages;
using PubSub;
using System;
using System.Net;
using System.Net.Sockets;

namespace Blockcore.Platform.Networking.Handlers
{
    public class ChatMessageOrchestratorHandler : IMessageOrchestratorHandler, IHandle<ChatMessage>
    {
        private readonly Hub events;
        private readonly IOrchestratorManager connectionManager;

        public ChatMessageOrchestratorHandler(PubSub.Hub events, IOrchestratorManager connectionManager)
        {
            this.events = events;
            this.connectionManager = connectionManager;
        }

        public void Process(BaseMessage message, ProtocolType protocol, IPEndPoint endpoint = null, NetworkClient client = null)
        {
            ChatMessage msg = (ChatMessage)message;
            Console.WriteLine("Message from {0}:{1}: {2}", endpoint.Address, endpoint.Port, msg.Content);
        }
    }
}
