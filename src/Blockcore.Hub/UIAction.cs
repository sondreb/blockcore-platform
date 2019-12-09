using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Blockcore.Hub
{
    public class UIAction
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("body")]
        public string Body { get; set; }
    }
}
