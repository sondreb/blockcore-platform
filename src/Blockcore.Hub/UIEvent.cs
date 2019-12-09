using System.Text.Json.Serialization;

namespace Blockcore.Hub
{
    public class UIEvent
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("body")]
        public string Body { get; set; }
    }
}
