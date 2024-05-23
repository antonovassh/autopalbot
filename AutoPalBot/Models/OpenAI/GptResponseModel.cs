namespace AutoPalBot.Models.OpenAI;

public class GptResponseModel
{
    public string? Id { get; set; }
    public string? Object { get; set; }
    public int Created { get; set; }
    public string? Model { get; set; }
    public IList<Choice>? Choices { get; set; }
    public string? SystemFingerprint { get; set; }
}
