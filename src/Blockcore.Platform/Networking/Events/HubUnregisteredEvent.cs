using Blockcore.Platform.Networking.Entities;
using Blockcore.Platform.Networking.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blockcore.Platform.Networking.Events
{
    public class HubUnregisteredEvent : IEvent
    {
        public HubUnregisteredEvent()
        {

        }

        public HubInfoMessage Data { get; set; }

        //public HubUnregisteredEvent(HubInfo hubInfo)
        //{
        //    this.Hub = hubInfo;
        //}

        //public HubInfo Hub { get; }
    }
}
