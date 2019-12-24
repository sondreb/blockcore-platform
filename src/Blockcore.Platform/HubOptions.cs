using System;
using System.Collections.Generic;
using System.Text;

namespace Blockcore.Platform
{
    public class HubOptions
    {
        public string Gateway { get; set; }

        public string AddNode { get; set; }

        public List<string> Hubs { get; set; }
    }
}
