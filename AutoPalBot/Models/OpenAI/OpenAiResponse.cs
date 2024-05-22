using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace AutoPalBot.Models.OpenAI;

public class OpenAIResponse
{
    public List<Choice> Choices { get; set; }
}
