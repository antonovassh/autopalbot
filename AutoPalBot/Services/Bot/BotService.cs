using AutoPalBot.Models.OpenAI;
using AutoPalBot.Services.DocumentGenerator;
using AutoPalBot.Services.Mindee;
using AutoPalBot.Services.OpenAI;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace AutoPalBot.Services.Bot;

public class BotService : IBotService
{
    private readonly IOpenAIService _openAIService;
    private readonly IDocumentService _documentService;
    //private readonly IMindeeService _mindeeService;
    public BotService(IOpenAIService openAISerice, IDocumentService documentService) //, IMindeeService mindeeService)
    {
        _openAIService = openAISerice;
        _documentService = documentService;

        //_mindeeService = mindeeService;
    }

    private enum BotState
    {
        AwaitingPassportNumber,
        AwaitingVehicleNumber,
        ConfirmingDocuments,
        AwaitingPriceConfirmation,
        GeneratingInsuranse
    }

    private BotState _currentState = BotState.AwaitingPassportNumber;
    private string _userPassportNumber;
    private string _userVehicleNumber;

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message)
            return;
        if (message.Text is not { } messageText)
            return;

        var chatId = message.Chat.Id;

        switch (_currentState)
        {

            case BotState.AwaitingPassportNumber:
                await HandlePassportNumberAsync(botClient, chatId, messageText);
                break;
            case BotState.AwaitingVehicleNumber:
                await HandleVehicleNumberAsync(botClient, chatId, messageText);
                break;
            case BotState.ConfirmingDocuments:
                await HandleDocumentConfirmationAsync(botClient, chatId, messageText);
                break;
            case BotState.AwaitingPriceConfirmation:
                await HandlePriceConfirmationAsync(botClient, chatId, messageText);
                break;
            case BotState.GeneratingInsuranse:
                await HandleInsuranseGenerationAsync(botClient, chatId, messageText);
                break;

        }
    }
    private async Task HandlePassportNumberAsync(ITelegramBotClient botClient, long chatId, string messageText)
    {
        await botClient.SendTextMessageAsync(chatId,
            "Welcome! I am AutoPalBot.\r\n" +
            "I will help you to make your car insuranse." +
            "\r\nPlease send a photo of your passport.");

        _currentState = BotState.AwaitingVehicleNumber;

    }

    private async Task HandleVehicleNumberAsync(ITelegramBotClient botClient, long chatId, string messageText)
    {
        _userPassportNumber = messageText;
        await botClient.SendTextMessageAsync(chatId,
            "Thank you! Now, please enter your vehicle identification number.");

        _currentState = BotState.ConfirmingDocuments;
    }

    private async Task HandleDocumentConfirmationAsync(ITelegramBotClient botClient, long chatId, string messageText)
    {
        _userVehicleNumber = messageText;

        var confirmationMessage = $"Please confirm your details:" +
            $"\nPassport Number: {_userPassportNumber}" +
            $"\nVehicle Identification Number: {_userVehicleNumber}" +
            $"\nIs this information correct? (yes/no)";

        await botClient.SendTextMessageAsync(chatId, confirmationMessage);

        _currentState = BotState.AwaitingPriceConfirmation;
    }

    private async Task HandlePriceConfirmationAsync(ITelegramBotClient botClient, long chatId, string messageText)
    {
        if (messageText.ToLower() == "yes")
        {
            await botClient.SendTextMessageAsync(chatId,
                "The fixed price for the insurance is 100 USD. Do you agree? (yes/no)");
            _currentState = BotState.GeneratingInsuranse;
        }
        else if (messageText.ToLower() == "no")
        {
            _currentState = BotState.AwaitingPassportNumber;
            _userPassportNumber = null;
            _userVehicleNumber = null;
            await botClient.SendTextMessageAsync(chatId,
                "Let's try again. Please send photo of your passport.");
            _currentState = BotState.AwaitingVehicleNumber;
        }
    }

    private async Task HandleInsuranseGenerationAsync(ITelegramBotClient botClient, long chatId, string messageText)
    {
        if (messageText.ToLower() == "yes")
        {
            var prompt = new TextGenerationRequestModel()
            {
                Model = "gpt-3.5-turbo",
                messages = new List<TextGenerationMessageModel>() {
                new TextGenerationMessageModel()
                {
                    Content = $"Generate a car insurance document for passport number " +
                    $"{_userPassportNumber} and vehicle number {_userVehicleNumber}.",
                    Role = "user"
                },
            }
            };

            var insuranse = await _openAIService.GenerateText(prompt);
            Console.WriteLine(insuranse);

            using (var pdfAsStream = _documentService.GenerateDocument(insuranse))
            {
                await botClient.SendDocumentAsync(chatId, new InputFileStream(pdfAsStream, "CarInsuranse.pdf"));
            }
            _currentState = BotState.AwaitingPassportNumber;

        }
        else if (messageText.ToLower() == "no")
        {
            await botClient.SendTextMessageAsync(chatId, "We apologize, but the price is fixed at 100 USD.");
            _currentState = BotState.AwaitingPassportNumber;
        }
    }

}


