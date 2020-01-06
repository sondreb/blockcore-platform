using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blockcore.Platform.Networking.Messages
{
    [MessagePackObject]
    public class HubNotFoundMessage : BaseMessage
    {
        public override ushort Command => MessageTypes.NOT_FOUND;

        [Key(1)]
        public string NotFoundId { get; set; }

        public HubNotFoundMessage()
        {

        }

        public HubNotFoundMessage(string senderId, string notFoundId)
        {
            Id = senderId;
            NotFoundId = notFoundId;
        }
    }
}
