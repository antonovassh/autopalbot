using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPalBot.Models.OpenAI;

public class OpenAIRequest
{
    public string prompt { get; set; }
    public int max_tokens { get; set; } = 100;
}
