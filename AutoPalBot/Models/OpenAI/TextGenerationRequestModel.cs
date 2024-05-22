using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPalBot.Models.OpenAI;

public class TextGenerationRequestModel
{
    [JsonProperty("model")]
    public string Model { get; set; }

    [JsonProperty("messages")]
    public ICollection<TextGenerationMessageModel> messages { get; set; }

}
