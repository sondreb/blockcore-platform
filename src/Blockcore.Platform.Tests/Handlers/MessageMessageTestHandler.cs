using Blockcore.Platform.Networking;
using Blockcore.Platform.Networking.Messages;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Blockcore.Platform.Tests.Handlers
{
    public class MessageMessageTestHandler : IMessageHandler, IHandle<ChatMessage>
    {
        public void Process(BaseMessage message, ProtocolType protocol, IPEndPoint endpoint = null, NetworkClient client = null)
        {
            
        }
    }
}
