﻿using Blockcore.Platform.Networking.Messages;

namespace Blockcore.Platform.Networking.Entities
{
    public class Message : BaseEntity
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Content { get; set; }
        public string RecipientId { get; set; }

        public Message(string from, string to, string content, string recipientId)
        {
            From = from;
            To = to;
            Content = content;
            RecipientId = recipientId;
        }

        public Message(MessageMessage message)
        {
            this.From = message.From;
            this.To = message.To;
            this.Content = message.Content;
            this.Id = message.Id;
            this.RecipientId = message.RecipientId;
        }

        public override BaseMessage ToMessage()
        {
            var msg = new MessageMessage();

            msg.From = From;
            msg.To = To;
            msg.Content = Content;
            msg.Id = Id;
            msg.RecipientId = RecipientId;

            return msg;
        }
    }
}
