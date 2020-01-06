using Blockcore.Platform.Networking.Entities;

namespace Blockcore.Platform.Networking.Events
{
    public class MessageReceivedEvent : IEvent
    {
        public string From { get; set; }

        public string To { get; set; }

        public string Content { get; set; }

        public Chat Data { get; set; }
    }
}
