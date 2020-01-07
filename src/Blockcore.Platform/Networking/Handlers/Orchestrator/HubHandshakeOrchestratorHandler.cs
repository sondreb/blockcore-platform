﻿using Blockcore.Platform.Networking.Entities;
using Blockcore.Platform.Networking.Messages;
using System.Net;
using System.Net.Sockets;

namespace Blockcore.Platform.Networking.Handlers
{
    /// <summary>
    /// This handler will take a handshake request from one hub and forward it to another hub.
    /// </summary>
    public class HubHandshakeOrchestratorHandler : IMessageOrchestratorHandler, IHandle<HubHandshakeMessage>
    {
        private readonly IOrchestratorManager manager;

        public HubHandshakeOrchestratorHandler(IOrchestratorManager manager)
        {
            this.manager = manager;
        }

        public void Process(BaseMessage message, ProtocolType Protocol, IPEndPoint endpoint = null, NetworkClient client = null)
        {
            HubHandshakeMessage req = (HubHandshakeMessage)message;

            // Query the active connections and see if we have a hub connected with that public key.
            HubInfo targetHub = manager.Connections.GetConnection(req.TargetId);

            if (targetHub != null)
            {
                manager.SendTCP(new HubHandshake(req), targetHub.Client);
            }
            else // If we cannot find the hub, we'll reply with a not found message.
            {
                HubInfo senderHub = manager.Connections.GetConnection(req.Id);
                manager.SendTCP(new HubNotFound(req.Id, req.TargetId), senderHub.Client);
            }
        }
    }
}
