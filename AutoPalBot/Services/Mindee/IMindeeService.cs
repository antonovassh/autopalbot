namespace AutoPalBot.Services.Mindee;

public interface IMindeeService
{
    Task<string> ExtractDataByEndpointNameAsync(byte[] fileBites, string fileName, string endpointName);
}
