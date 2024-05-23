namespace AutoPalBot.Services.OpenAI;

public interface IOpenAIService
{
    Task<bool> EnsureSentenceIsPositive(string sentence);

    Task<string> GenerateCanInsurance(string passportNumber, string vehicleNumber);
}
