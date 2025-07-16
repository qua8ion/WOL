using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Dto.Services.Auth;

public record LoginResponseModel
{
    /// <summary>
    /// JWT токен
    /// </summary>
    [JsonProperty("accessToken")]
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = null!;

    /// <summary>
    /// Имя пользователя
    /// </summary>
    [JsonProperty("username")]
    [JsonPropertyName("username")]
    public string Username { get; set; } = null!;
}