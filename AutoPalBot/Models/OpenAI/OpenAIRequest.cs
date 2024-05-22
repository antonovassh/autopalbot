using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace AutoPalBot.Models.OpenAI;

public class OpenAIRequest
{
    public string Model { get; set; } = "gpt-3.5-turbo";

    public TextGenerationRequestModel Prompt { get; set; }

    public int Max_tokens { get; set; } = 100;

    public ICollection<Message> Messages { get; set; }
}
