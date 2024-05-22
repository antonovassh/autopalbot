using AutoPalBot.Models.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPalBot.Services.OpenAI;

public interface IOpenAIService
{
    public Task<string> GenerateText(TextGenerationRequestModel prompt);

    public Task<HttpResponseMessage> HttpRawRequest(TextGenerationRequestModel prompt);
}
