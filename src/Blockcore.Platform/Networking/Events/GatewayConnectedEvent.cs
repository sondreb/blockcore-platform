using Blockcore.Platform.Networking.Messages;

namespace Blockcore.Platform.Networking.Events
{
    public class GatewayConnectedEvent : IEvent
    {
        public string Name { get; set; }

        public HubInfoMessage Self { get; set; }
    }
}
