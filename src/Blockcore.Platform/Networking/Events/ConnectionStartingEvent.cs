using Blockcore.Platform.Networking.Messages;

namespace Blockcore.Platform.Networking.Events
{
    public class ConnectionStartingEvent : IEvent
    {
        public HubInfoMessage Data { get; set; }
    }
}
