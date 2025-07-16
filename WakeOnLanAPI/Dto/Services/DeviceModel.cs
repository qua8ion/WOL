using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Dto.Services
{
    [Serializable]
    public record DeviceModel
    {
        /// <summary>
        /// IP адрес устрйоства в локальной сети
        /// </summary>
        [JsonPropertyName("ipV4")]
        [JsonProperty("ipV4")]
        public string IpV4 { get; set; } = null!;

        /// <summary>
        /// Физический адрес устройства
        /// </summary>
        [JsonPropertyName("mac")]
        [JsonProperty("mac")]
        public string Mac { get; set; } = null!;

        /// <summary>
        /// Имя устройства
        /// </summary>
        [JsonPropertyName("name")]
        [JsonProperty("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Онлайн устройства
        /// </summary>
        [JsonPropertyName("online")]
        [JsonProperty("online")]
        public bool Online { get; set; } = false;

        /// <summary>
        /// Описание
        /// </summary>
        [JsonPropertyName("description")]
        [JsonProperty("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Дата и время последнего обновления данных
        /// </summary>
        [JsonPropertyName("lastScan")]
        [JsonProperty("lastScan")]
        public DateTime LastScan { get; set; }
    }
}
