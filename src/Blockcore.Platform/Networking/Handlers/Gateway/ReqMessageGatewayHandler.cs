using Blockcore.Platform.Networking.Entities;
using Blockcore.Platform.Networking.Messages;
using System.Net;
using System.Net.Sockets;

namespace Blockcore.Platform.Networking.Handlers
{
    public class ReqMessageGatewayHandler : IMessageOrchestratorHandler, IHandle<ReqMessage>
    {
        
        private readonly IOrchestratorManager manager;

        public ReqMessageGatewayHandler(IOrchestratorManager manager)
        {
            this.manager = manager;
        }

        public void Process(BaseMessage message, ProtocolType Protocol, IPEndPoint endpoint = null, NetworkClient client = null)
        {
            ReqMessage req = (ReqMessage)message;

            HubInfo hubInfo = manager.Connections.GetConnection(req.RecipientId);

            if (hubInfo != null)
            {
                manager.SendTCP(new Req(req), hubInfo.Client);
            }
        }
    }
}
