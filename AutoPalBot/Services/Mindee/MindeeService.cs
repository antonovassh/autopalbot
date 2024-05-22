using RestSharp;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using AutoPalBot.Models;

namespace AutoPalBot.Services.Mindee;

public static class MindeeService
{
    public static async Task<DocumentData> ExtractDataAsync(string imagePath)
    {
        var client = new RestClient("https://api.mindee.net/v1/products/your-mindee-product/parse");
        var request = new RestRequest
        {
            Method = Method.Post
        };
        request.AddHeader("Authorization", "28e01ae4e6358b47b1501de63fc89bb4");
        request.AddFile("document", imagePath, "image/jpeg"); // Specify the MIME type

        var response = await client.ExecuteAsync(request);
        var jsonResponse = JObject.Parse(response.Content);

        var documentData = new DocumentData
        {
            PassportNumber = jsonResponse["document"]["ID Number"]["passport_number"].ToString(),
            VehicleIdentificationNumber = jsonResponse["document"]["inference"]["vehicle_identification_number"].ToString()
        };

        return documentData;
    }
}
