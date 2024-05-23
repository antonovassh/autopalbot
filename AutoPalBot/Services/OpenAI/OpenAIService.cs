﻿using AutoPalBot.Models.OpenAI;
using Newtonsoft.Json;
using System.Text;

namespace AutoPalBot.Services.OpenAI;

public class OpenAIService : IOpenAIService
{

    private const string apiKey = "OpenAiKey";

    public async Task<bool> EnsureSentenceIsPositive(string sentence)
    {
        var messages = new List<TextGenerationMessageModel>()
        {
             new TextGenerationMessageModel()
            {
                 //TODO: move string to resources
                Content = "Answer only true ot false. " +
                    "If the sentence expresses agreement like ok or yes then answer true. Otherwise answer false",
                Role = "system"
            },
            new TextGenerationMessageModel()
            {
                Content = sentence,
                Role = "user"
            }
        };

        var response = await GenerateText(messages);

        if(bool.TryParse(response, out var result)) 
        {  
            return result; 
        }
        else
        {
            //TODO: create EnsureSentenceIsPositiveException then handle it. 
            return false;
        }
    }

    public Task<string> GenerateCanInsurance(string passportNumber, string vehicleNumber)
    {
        //TODO: move string to resources
        var promt = $"Generate a car insurance document for passport number {passportNumber} and vehicle number {vehicleNumber}.";

        var messages = new List<TextGenerationMessageModel>()
        {
            new TextGenerationMessageModel()
            {
                Content = promt,
                Role = "user"
            }
        };

        return GenerateText(messages);
    }

    private Task<HttpResponseMessage> HttpRawRequest(TextGenerationRequestModel prompt)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        string jsonContent = JsonConvert.SerializeObject(prompt);
        request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        return client.SendAsync(request);
    }

    private async Task<string> GenerateText(List<TextGenerationMessageModel> messages)
    {
        var generationModel = new TextGenerationRequestModel()
        {
            Model = "gpt-4o",
            Messages = messages
        };

        HttpResponseMessage response = await HttpRawRequest(generationModel);

        var responseContentJson = await response.Content.ReadAsStringAsync();

        var result = JsonConvert.DeserializeObject<GptResponseModel>(responseContentJson);

        return result!.Choices!.First().Message!.Content!;
    }
}
