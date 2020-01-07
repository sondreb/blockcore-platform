using Blockcore.Platform.Networking.Entities;
using Blockcore.Platform.Networking.Events;
using Blockcore.Platform.Networking.Messages;
using PubSub;
using System.Net;
using System.Net.Sockets;

namespace Blockcore.Platform.Networking.Handlers
{
    public class HubHandshakeHandler : IMessageHandler, IHandle<HubHandshakeMessage>
    {
        private readonly Hub events;
        private readonly IHubManager manager;

        public HubHandshakeHandler(PubSub.Hub events, IHubManager manager)
        {
            this.events = events;
            this.manager = manager;
        }

        public void Process(BaseMessage message, ProtocolType Protocol, IPEndPoint endpoint = null, NetworkClient client = null)
        {
            HubHandshakeMessage msg = (HubHandshakeMessage)message;
            HubInfo hubInfo = manager.Connections.GetConnection(msg.TargetId);


            // Decrypt the payload with our private key. It has been encrypted by the caller using our public key.
            var cipher = msg.Payload;
            var decryptedMessage = this.manager.Identity.Decrypt(cipher);

            // Based on configuration, the hub instance might accept this handshake outright without any additional verification,
            // or the hub can rely on manual public key approval, or manual acceptance of incomign handshake requests.
            if (manager.TrustedHubs.Contains(hubInfo.Id))
            {
                // Yeeh!
                // If we have an incoming handshake request, should we initiate a connection outbound? Or should we just say "handhsake ok"?
            }
            else
            {
                manager.HubRequests.Add(new HubHandshake(msg));




            }

            //if (hubInfo != null)
            //{
            //    events.Publish(new ConnectionStartingEvent() { Data = (HubInfoMessage)hubInfo.ToMessage() });

            //    IPEndPoint ResponsiveEP = manager.FindReachableEndpoint(hubInfo);

            //    if (ResponsiveEP != null)
            //    {
            //        events.Publish(new HubConnectionStartedEvent() { 
            //            TargetId = msg.TargetId,
            //            OriginId = msg.Id,
            //            Data = (HubInfoMessage)hubInfo.ToMessage(), 
            //            Endpoint = ResponsiveEP.ToString() });;

            //        events.Publish(new ConnectionUpdatedEvent() {
            //            TargetId = msg.TargetId,
            //            OriginId = msg.Id,
            //            Data = (HubInfoMessage)hubInfo.ToMessage()
            //        });
            //    }
            //}
        }
    }
}
