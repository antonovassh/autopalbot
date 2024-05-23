using Newtonsoft.Json;

namespace AutoPalBot.Models.OpenAI;

public class TextGenerationMessageModel
{
    [JsonProperty("role")]
    public string? Role { get; set; }

    [JsonProperty("content")]
    public string? Content { get; set; }
}
