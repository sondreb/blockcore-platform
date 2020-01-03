using System;
using System.Collections.Generic;
using System.Text;

namespace Blockcore.Platform
{
    public class OrchestratorOptions
    {
        public ushort Port { get; set; } = 6610; // Currently not used in the port list: https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers
    }
}
