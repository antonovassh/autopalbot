﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPalBot.Models.OpenAI;

public class Message
{
    public string Role { get; set; }
    public string Content { get; set; }
}
