using Newtonsoft.Json;

namespace AutoPalBot.Models.OpenAI;

public class TextGenerationRequestModel
{
    [JsonProperty("model")]
    public string? Model { get; set; }

    [JsonProperty("messages")]
    public ICollection<TextGenerationMessageModel>? Messages { get; set; }
}
