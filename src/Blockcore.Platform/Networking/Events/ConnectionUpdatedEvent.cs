using Blockcore.Platform.Networking.Messages;

namespace Blockcore.Platform.Networking.Events
{
    public class ConnectionUpdatedEvent : IEvent
    {
        public HubInfoMessage Data { get; set; }
    }
}
