using Blockcore.Platform.Networking.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blockcore.Platform.Networking.Events
{
    public class HubConnectEvent : IEvent
    {
        public HubConnectEvent()
        {

        }

        public HubConnectEvent(HubInfo hub)
        {
            this.Hub = hub;
        }

        public HubInfo Hub { get;  }
    }
}
