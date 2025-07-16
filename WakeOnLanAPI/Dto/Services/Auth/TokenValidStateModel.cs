using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Dto.Services.Auth;

public record TokenValidStateModel
{
    [JsonProperty("accessToken")]
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = null!;

    [JsonProperty("valid")]
    [JsonPropertyName("valid")]
    public bool Valid { get; set; } = false;
}