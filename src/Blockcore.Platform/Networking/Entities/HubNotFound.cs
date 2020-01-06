using Blockcore.Platform.Networking.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blockcore.Platform.Networking.Entities
{
    public class HubNotFound : BaseEntity
    {
        public string NotFoundId { get; set; }

        public HubNotFound(string senderId, string notFoundId)
        {
            this.Id = senderId;
            this.NotFoundId = notFoundId;
        }

        public HubNotFound(HubNotFoundMessage message)
        {
            this.Id = message.Id;
            this.NotFoundId = message.NotFoundId;
        }

        public override BaseMessage ToMessage()
        {
            var msg = new HubNotFoundMessage();

            msg.Id = this.Id;
            msg.NotFoundId = this.NotFoundId;

            return msg;
        }
    }
}
