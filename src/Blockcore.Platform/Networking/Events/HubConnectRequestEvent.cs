using Blockcore.Platform.Networking.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blockcore.Platform.Networking.Events
{
    public class HubConnectRequestEvent : IEvent
    {
        public HubConnectRequestEvent()
        {

        }

        public HubConnectRequestEvent(HubInfo hub)
        {
            this.Hub = hub;
        }

        public HubInfo Hub { get; }
    }
}
