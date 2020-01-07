using Blockcore.Platform.Networking.Messages;

namespace Blockcore.Platform.Networking.Entities
{
    public class HubHandshake : BaseEntity
    {
        public string TargetId { get; set; }

        public HubInfoMessage HubInfo { get; set; }

        public string Payload { get; set; }

        public HubHandshake(string originId, string targetId)
        {
            Id = originId;
            TargetId = targetId;
        }

        public HubHandshake(HubHandshakeMessage message)
        {
            this.Id = message.Id;
            this.TargetId = message.TargetId;
        }

        public override BaseMessage ToMessage()
        {
            var msg = new HubHandshakeMessage(Id, TargetId);
            return msg;
        }
    }
}
