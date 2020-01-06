using Blockcore.Platform.Networking.Events;
using Blockcore.Platform.Networking.Messages;
using PubSub;
using System.Net;
using System.Net.Sockets;

namespace Blockcore.Platform.Networking.Handlers
{
    public class NotificationHandler : IMessageHandler, IHandle<NotificationMessage>
    {
        private readonly Hub events;
        private readonly IHubManager manager;

        public NotificationHandler(PubSub.Hub events, IHubManager manager)
        {
            this.events = events;
            this.manager = manager;
        }

        public void Process(BaseMessage message, ProtocolType Protocol, IPEndPoint endpoint = null, NetworkClient client = null)
        {
            NotificationMessage item = (NotificationMessage)message;

            if (item.Type == NotificationsTypes.Disconnected)
            {
                var hubInfo = manager.Connections.GetConnection(item.Tag.ToString());

                if (hubInfo != null)
                {
                    manager.Connections.RemoveConnection(hubInfo);
                    events.Publish(new ConnectionRemovedEvent() { Data = (HubInfoMessage)hubInfo.ToMessage() });
                }
            }
            else if (item.Type == NotificationsTypes.ServerShutdown)
            {
                manager.DisconnectOrchestrator(false);
                events.Publish(new OrchestratorShutdownEvent());
            }
        }
    }
}
