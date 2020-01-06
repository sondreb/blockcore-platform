using MessagePack;

namespace Blockcore.Platform.Networking.Messages
{
    [MessagePackObject]
    public class HubConnectRequestMessage : BaseMessage
    {
        public override ushort Command => MessageTypes.CONNECT_REQUEST;

        [Key(1)]
        public string TargetId { get; set; }

        public HubConnectRequestMessage()
        {

        }

        public HubConnectRequestMessage(string originId, string targetId)
        {
            Id = originId;
            TargetId = targetId;
        }
    }
}
