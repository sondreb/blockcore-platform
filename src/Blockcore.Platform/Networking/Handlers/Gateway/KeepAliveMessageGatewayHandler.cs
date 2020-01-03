using Blockcore.Platform.Networking.Messages;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace Blockcore.Platform.Networking.Handlers
{
    public class KeepAliveMessageGatewayHandler : IMessageOrchestratorHandler, IHandle<KeepAliveMessage>
    {
        private readonly ILogger<InfoMessageGatewayHandler> log;
        private readonly IOrchestratorManager manager;

        public KeepAliveMessageGatewayHandler(ILogger<InfoMessageGatewayHandler> log, IOrchestratorManager manager)
        {
            this.log = log;
            this.manager = manager;
        }

        public void Process(BaseMessage message, ProtocolType protocol, IPEndPoint endpoint = null, NetworkClient client = null)
        {
            // This doesn't do anything, but nodes will send Keep Alive.
        }
    }
}
