using MessagePack;

namespace Blockcore.Platform.Networking.Messages
{
    [MessagePackObject]
    public class ChatMessage: BaseMessage
    {
        public override ushort Command => MessageTypes.CHAT;

        [Key(1)]
        public string From { get; set; }

        [Key(2)]
        public string To { get; set; }

        [Key(3)]
        public string Content { get; set; }

        [Key(4)]
        public string RecipientId { get; set; }

        public ChatMessage()
        {

        }

        public ChatMessage(string from, string to, string content, string recipientId)
        {
            From = from;
            To = to;
            Content = content;
            RecipientId = recipientId;
        }
    }
}
