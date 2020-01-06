using Blockcore.Platform.Networking.Entities;
using Blockcore.Platform.Networking.Events;
using Blockcore.Platform.Networking.Messages;
using Microsoft.Extensions.Logging;
using PubSub;
using System.Net;
using System.Net.Sockets;

namespace Blockcore.Platform.Networking.Handlers
{
    public class HubNotFoundHandler : IMessageHandler, IHandle<HubNotFoundMessage>
    {
        private readonly Hub events;
        private readonly IHubManager manager;
        private readonly ILogger<HubNotFoundHandler> log;

        public HubNotFoundHandler(ILogger<HubNotFoundHandler> log, PubSub.Hub events, IHubManager manager)
        {
            this.log = log;
            this.events = events;
            this.manager = manager;
        }

        public void Process(BaseMessage message, ProtocolType Protocol, IPEndPoint endpoint = null, NetworkClient client = null)
        {
            HubNotFoundMessage msg = (HubNotFoundMessage)message;
            log.LogWarning("The attempt to connect with {HubId} was rejected due to hub not found.", msg.NotFoundId);
            
            //HubInfo hubInfo = manager.Connections.GetConnection(msg.TargetId);

            //if (hubInfo != null)
            //{
            //    events.Publish(new ConnectionStartingEvent() { Data = (HubInfoMessage)hubInfo.ToMessage() });

            //    IPEndPoint ResponsiveEP = manager.FindReachableEndpoint(hubInfo);

            //    if (ResponsiveEP != null)
            //    {
            //        events.Publish(new HubConnectionStartedEvent() { 
            //            TargetId = msg.TargetId,
            //            OriginId = msg.Id,
            //            Data = (HubInfoMessage)hubInfo.ToMessage(), 
            //            Endpoint = ResponsiveEP.ToString() });;

            //        events.Publish(new ConnectionUpdatedEvent() {
            //            TargetId = msg.TargetId,
            //            OriginId = msg.Id,
            //            Data = (HubInfoMessage)hubInfo.ToMessage()
            //        });
            //    }
            //}
        }
    }
}
