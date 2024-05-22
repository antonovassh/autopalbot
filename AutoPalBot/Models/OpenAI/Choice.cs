using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPalBot.Models.OpenAI;

public class Choice
{
    public int Index { get; set; }
    public Message Message { get; set; }
}
