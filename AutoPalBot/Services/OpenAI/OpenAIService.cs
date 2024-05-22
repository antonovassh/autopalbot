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
    private readonly string apiKey;
    private readonly string apiUrl = "https://api.openai.com/v1/completions";

    public OpenAIService(string apiKey)
    {
        this.apiKey = apiKey;
    }

    public async Task<string> GenerateCarInsuranceDocument(string prompt)
    {
        var client = new RestClient(apiUrl);
        var request = new RestRequest("/v1/completions", Method.Post);

        request.AddHeader("Authorization", $"Bearer {apiKey}");
        request.AddHeader("Content-Type", "application/json");

        var openAIRequest = new OpenAIRequest { prompt = prompt };
        request.AddJsonBody(openAIRequest);

        var response = await client.ExecuteAsync(request);
        if (response.IsSuccessful)
        {
            var openAIResponse = JsonConvert.DeserializeObject<OpenAIResponse>(response.Content);
            return openAIResponse.choices.FirstOrDefault()?.text.Trim();
        }

        throw new Exception("Error calling OpenAI API: " + response.ErrorMessage);
    }
}
