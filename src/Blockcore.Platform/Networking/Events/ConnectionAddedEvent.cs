using Blockcore.Platform.Networking.Messages;

namespace Blockcore.Platform.Networking.Events
{
    public class ConnectionAddedEvent : IEvent
    {
        public HubInfoMessage Data { get; set; }
    }
}
