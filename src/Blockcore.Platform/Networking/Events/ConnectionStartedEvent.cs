using Blockcore.Platform.Networking.Messages;

namespace Blockcore.Platform.Networking.Events
{
    public class ConnectionStartedEvent : IEvent
    {
        public HubInfoMessage Data { get; set; }

        public string Endpoint { get; set; }
    }
}
