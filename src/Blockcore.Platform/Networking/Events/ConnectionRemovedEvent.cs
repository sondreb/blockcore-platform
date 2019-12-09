using Blockcore.Platform.Networking.Messages;

namespace Blockcore.Platform.Networking.Events
{
    public class ConnectionRemovedEvent : IEvent
    {
        public HubInfoMessage Data { get; set; }
    }
}
