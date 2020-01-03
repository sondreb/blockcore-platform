using Blockcore.Platform.Networking.Entities;
using Blockcore.Platform.Networking.Events;
using Blockcore.Platform.Networking.Messages;
using PubSub;
using System.Net;
using System.Net.Sockets;

namespace Blockcore.Platform.Networking.Handlers
{
    public class ReqMessageHandler : IMessageHandler, IHandle<ReqMessage>
    {
        private readonly Hub events;
        private readonly IHubManager manager;

        public ReqMessageHandler(PubSub.Hub events, IHubManager manager)
        {
            this.events = events;
            this.manager = manager;
        }

        public void Process(BaseMessage message, ProtocolType Protocol, IPEndPoint endpoint = null, NetworkClient client = null)
        {
            ReqMessage msg = (ReqMessage)message;
            HubInfo hubInfo = manager.Connections.GetConnection(msg.RecipientId);

            if (hubInfo != null)
            {
                events.Publish(new ConnectionStartingEvent() { Data = (HubInfoMessage)hubInfo.ToMessage() });

                IPEndPoint ResponsiveEP = manager.FindReachableEndpoint(hubInfo);

                if (ResponsiveEP != null)
                {
                    events.Publish(new HubConnectionStartedEvent() { Data = (HubInfoMessage)hubInfo.ToMessage(), Endpoint = ResponsiveEP.ToString() });
                    events.Publish(new ConnectionUpdatedEvent() { Data = (HubInfoMessage)hubInfo.ToMessage() });
                }
            }
        }
    }
}
