using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPalBot.Services.OpenAI;

public interface IOpenAIService
{
    Task<string> GenerateCarInsuranceDocument(string prompt);
}
