using MessagePack;

namespace Blockcore.Platform.Networking.Messages
{
    [MessagePackObject]
    public class HubHandshakeMessage : BaseMessage
    {
        public override ushort Command => MessageTypes.CONNECT_APPROVAL;

        [Key(1)]
        public string TargetId { get; set; }

        [Key(2)]
        public string Payload { get; set; }

        public HubHandshakeMessage()
        {

        }

        public HubHandshakeMessage(string originId, string targetId)
        {
            Id = originId;
            TargetId = targetId;
        }
    }
}
