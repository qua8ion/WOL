using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Dto.Services.Auth
{
    public record AuthModel
    {

        /// <summary>
        /// Имя пользователя
        /// </summary>
        [JsonPropertyName("username")]
        [JsonProperty("username")]
        public string UserName { get; set; } = null!;

        /// <summary>
        /// Пароль
        /// </summary>
        [JsonPropertyName("password")]
        [JsonProperty("password")]
        public string Password { get; set; } = null!;
    }
}
