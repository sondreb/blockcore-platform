using Blockcore.Platform.Networking.Entities;
using Blockcore.Platform.Networking.Events;
using Blockcore.Platform.Networking.Messages;
using PubSub;
using System.Net;
using System.Net.Sockets;

namespace Blockcore.Platform.Networking.Handlers
{
    public class HubConnectRequestHandler : IMessageHandler, IHandle<HubConnectRequestMessage>
    {
        private readonly Hub events;
        private readonly IHubManager manager;

        public HubConnectRequestHandler(PubSub.Hub events, IHubManager manager)
        {
            this.events = events;
            this.manager = manager;
        }

        public void Process(BaseMessage message, ProtocolType Protocol, IPEndPoint endpoint = null, NetworkClient client = null)
        {
            HubConnectRequestMessage msg = (HubConnectRequestMessage)message;
            HubInfo hubInfo = manager.Connections.GetConnection(msg.TargetId);

            if (hubInfo != null)
            {
                events.Publish(new ConnectionStartingEvent() { Data = (HubInfoMessage)hubInfo.ToMessage() });

                IPEndPoint ResponsiveEP = manager.FindReachableEndpoint(hubInfo);

                if (ResponsiveEP != null)
                {
                    events.Publish(new HubConnectionStartedEvent() { 
                        TargetId = msg.TargetId,
                        OriginId = msg.Id,
                        Data = (HubInfoMessage)hubInfo.ToMessage(), 
                        Endpoint = ResponsiveEP.ToString() });;

                    events.Publish(new ConnectionUpdatedEvent() {
                        TargetId = msg.TargetId,
                        OriginId = msg.Id,
                        Data = (HubInfoMessage)hubInfo.ToMessage()
                    });
                }
            }
        }
    }
}
