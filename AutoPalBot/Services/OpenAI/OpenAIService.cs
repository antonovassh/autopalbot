using AutoPalBot.Models.OpenAI;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPalBot.Services.OpenAI;

public class OpenAIService : IOpenAIService
{
    public const string apiKey = "sk-proj-fHWauI760cF2C2J5xk8FT3BlbkFJ1ZUMPwWkJFiMXM9k4nAQ";


    public async Task<HttpResponseMessage> HttpRawRequest(TextGenerationRequestModel prompt)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        string jsonContent = JsonConvert.SerializeObject(prompt);
        request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        return await client.SendAsync(request);
    }

    public async Task<string> GenerateText(TextGenerationRequestModel prompt)
    {
        HttpResponseMessage response = await HttpRawRequest(prompt);

        var responseContentJson = await response.Content.ReadAsStringAsync();

        return responseContentJson;

    }
}
