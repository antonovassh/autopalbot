using Mindee;
using Mindee.Http;
using Mindee.Input;
using Mindee.Product.Generated;
using System.Text.RegularExpressions;

namespace AutoPalBot.Services.Mindee;

public class MindeeService : IMindeeService
{
    //TODO: apiKey to secrets
    private const string apiKey = "MindeeKey";

    private readonly MindeeClient _mindeeClient;

    public MindeeService()
    {
        _mindeeClient = new MindeeClient(apiKey);
    }

    public async Task<string> ExtractDataByEndpointNameAsync(byte[] fileBites, string fileName, string endpointName)
    {
        var inputSource = new LocalInputSource(fileBites, fileName);

        CustomEndpoint endpoint = new(
                endpointName: endpointName,
                accountName: "antonovassh",
                version: "1"
            );

        var response = await _mindeeClient.EnqueueAndParseAsync<GeneratedV1>(inputSource, endpoint);

        return ExtractValue(response.Document.Inference.Prediction.ToString());
    }

    private string ExtractValue(string input)
    {
        string pattern = @":value:\s*([A-Z0-9]+)";

        Match match = Regex.Match(input, pattern);

        return match.Groups[1].Value;
    }
}