namespace AutoPalBot.Services.OpenAI;

public interface IOpenAIService
{
    Task<bool> EnsureSentenceIsPositive(string sentence);

    Task<bool> EnsureSentenceIsQuestion(string input);

    Task<string> AnswerAsInsuranceAgent(string input);

    Task<string> GenerateCanInsurance(string passportNumber, string vehicleNumber);
}
