using Blockcore.Platform.Networking.Messages;

namespace Blockcore.Platform.Networking.Events
{
    public class HubInfoEvent : IEvent
    {
        public HubInfoMessage Data { get; set; }
    }
}
