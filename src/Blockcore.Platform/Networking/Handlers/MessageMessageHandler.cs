﻿using Blockcore.Platform.Networking.Entities;
using Blockcore.Platform.Networking.Events;
using Blockcore.Platform.Networking.Messages;
using PubSub;
using System.Net;
using System.Net.Sockets;

namespace Blockcore.Platform.Networking.Handlers
{
    public class MessageMessageHandler : IMessageHandler, IHandle<MessageMessage>
    {
        private readonly Hub events;
        private readonly IHubManager connectionManager;

        public MessageMessageHandler(PubSub.Hub events, IHubManager connectionManager)
        {
            this.events = events;
            this.connectionManager = connectionManager;
        }

        public void Process(BaseMessage message, ProtocolType protocol, IPEndPoint endpoint = null, NetworkClient client = null)
        {
            MessageMessage msg = (MessageMessage)message;

            // Publish the event on the local event hub.
            //hub.Publish(new MessageReceivedEvent() { From = msg.From, To = msg.To, Content = msg.Content });
            //connectionManager.GetConnection(msg.Id);

            // TODO: Debug and figure out what to do here.
            if (string.IsNullOrWhiteSpace(msg.Id))
            {
                events.Publish(new MessageReceivedEvent() { Data = new Message(msg) });
                //OnResultsUpdate.Invoke(this, msg.From + ": " + msg.Content);
            }
            else if (endpoint != null & client != null)
            {
                events.Publish(new MessageReceivedEvent() { Data = new Message(msg) });
                //OnMessageReceived.Invoke(EP, new MessageReceivedEventArgs(CI, new Message(m), EP));
            }
        }
    }
}
