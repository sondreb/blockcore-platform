using Blockcore.Platform.Networking.Entities;
using Blockcore.Platform.Networking.Events;
using Blockcore.Platform.Networking.Messages;
using PubSub;
using System.Net;
using System.Net.Sockets;

namespace Blockcore.Platform.Networking.Handlers
{
    public class InfoMessageHandler : IMessageHandler, IHandle<HubInfoMessage>
    {
        private Hub events;
        private readonly IHubManager manager;

        public InfoMessageHandler(PubSub.Hub events, IHubManager manager)
        {
            this.events = events;
            this.manager = manager;
        }

        public void Process(BaseMessage message, ProtocolType Protocol, IPEndPoint endpoint = null, NetworkClient client = null)
        {
            var msg = (HubInfoMessage)message;

            // We need this lock as we're checking for null and during an initial handshake messages will come almost simultaneously.
            lock (manager.Connections)
            {
                HubInfo hubInfo = manager.Connections.GetConnection(msg.Id);

                if (hubInfo == null)
                {
                    hubInfo = new HubInfo(msg);
                    manager.Connections.AddConnection(hubInfo);

                    events.Publish(new HubRegisteredEvent() { Data = (HubInfoMessage)hubInfo.ToMessage() });
                }
                else
                {
                    hubInfo.Update(msg);

                    events.Publish(new HubUpdatedEvent() { Data = (HubInfoMessage)hubInfo.ToMessage() });
                }
            }
        }
    }
}
