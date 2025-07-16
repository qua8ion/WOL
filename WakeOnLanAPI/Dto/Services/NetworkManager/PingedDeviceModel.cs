using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Dto.Services.NetworkManager
{
    public record PingedDeviceModel
    {
        [JsonPropertyName("ip")]
        [JsonProperty("ip")]
        public string IP { get; set; } = null!;

        [JsonPropertyName("online")]
        [JsonProperty("online")]
        public bool Online { get; set; } = false;

        [JsonPropertyName("lastScan")]
        [JsonProperty("lastScan")]
        public DateTime LastScan { get; set; }
    }
}
