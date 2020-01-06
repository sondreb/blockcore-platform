using Blockcore.Platform.Networking.Messages;

namespace Blockcore.Platform.Networking.Entities
{
    public class HubConnectRequest : BaseEntity
    {
        public string TargetId { get; set; }

        public HubConnectRequest(string originId, string targetId)
        {
            Id = originId;
            TargetId = targetId;
        }

        public HubConnectRequest(HubConnectRequestMessage message)
        {
            this.Id = message.Id;
            this.TargetId = message.TargetId;
        }

        public override BaseMessage ToMessage()
        {
            var msg = new HubConnectRequestMessage(Id, TargetId);
            return msg;
        }
    }
}
