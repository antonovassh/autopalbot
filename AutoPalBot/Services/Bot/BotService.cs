using AutoPalBot.Models.OpenAI;
using AutoPalBot.Services.DocumentGenerator;
using AutoPalBot.Services.Mindee;
using AutoPalBot.Services.OpenAI;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Message = Telegram.Bot.Types.Message;

namespace AutoPalBot.Services.Bot;

public class BotService : IBotService
{
    private readonly IOpenAIService _openAIService;
    private readonly IDocumentService _documentService;
    private readonly IMindeeService _mindeeService;

    private BotState _currentState = BotState.StartingConversation;
    private string? _userPassportNumber;
    private string? _userVehicleNumber;

    public BotService(IOpenAIService openAISerice, IDocumentService documentService, IMindeeService mindeeService)
    {
        _openAIService = openAISerice;
        _documentService = documentService;
        _mindeeService = mindeeService;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message)
            return;

        switch (_currentState)
        {
            case BotState.StartingConversation:
                await RequestUserPassport(botClient, message.Chat.Id);
                break;
            case BotState.AwaitingPassportNumber:
                await ParsePassportPhotoAsync(botClient, message.Chat.Id, message);
                break;
            case BotState.AwaitingVehicleNumber:
                await ParseVehicleIdentificationPhotoAsync(botClient, message.Chat.Id, message);
                break;
            case BotState.AwaitingDataConfirmation:
                await CreateInvoiceAsync(botClient, message.Chat.Id, message);
                break;
            case BotState.AwaitingPriceConfirmation:
                await GeneraeleInsuranseAsync(botClient, message.Chat.Id, message);
                break;
        }
    }

    private async Task RequestUserPassport(ITelegramBotClient botClient, long chatId)
    {
        Chat chat = await botClient.GetChatAsync(chatId);
        string username = chat.Username;

        //TODO: move string to resources
        await botClient.SendTextMessageAsync(chatId,
               $"Hello {username}! I'm your virtual assistant from InsuranceCompany. " +
               "My name is AutoPalBot.I can help you quickly and easily get car insurance. " +
               "Just share a photo of your passport and vehicle ID document, " +
               "and I'll take care of the rest.Let's get started on securing your vehicle today!" +
               " Please, send me a photo of your passport.");

        _currentState = BotState.AwaitingPassportNumber;
    }

    private async Task ParsePassportPhotoAsync(ITelegramBotClient botClient, long chatId, Message message)
    {
        if (message.Photo != null)
        {
            string fileName = $"{message.Chat.Username}_passport.jpg";

            var photo = message.Photo[^1]; // Get the last (largest) photo
            var photoFile = await botClient.GetFileAsync(photo.FileId);
            var photoStream = new MemoryStream();

            await botClient.DownloadFileAsync(photoFile.FilePath!, photoStream);

            byte[] photoBytes = photoStream.ToArray();

            _userPassportNumber = await _mindeeService.ExtractDataByEndpointNameAsync(photoBytes, fileName, "passport");

            if (!string.IsNullOrEmpty(_userPassportNumber))
            {
                _currentState = BotState.AwaitingVehicleNumber;

                //TODO: move string to resources
                await botClient.SendTextMessageAsync(chatId,
                    "Thank you! Now, please send a photo of your vehicle identification document.");
            }
            else
            {
                await SendPhotoIncorrectAsync(botClient, chatId, BotState.AwaitingPassportNumber);
            }
        }
        else
        {
            await SendPhotoIncorrectAsync(botClient, chatId, BotState.AwaitingPassportNumber);
        }

    }

    private async Task ParseVehicleIdentificationPhotoAsync(ITelegramBotClient botClient, long chatId, Message message)
    {

        if (message.Photo != null)
        {
            string fileName = $"{message.Chat.Username}_car_license.jpg";

            var photo = message.Photo[^1]; // Get the last (largest) photo
            var photoFile = await botClient.GetFileAsync(photo.FileId);
            var photoStream = new MemoryStream();

            await botClient.DownloadFileAsync(photoFile.FilePath!, photoStream);

            byte[] photoBytes = photoStream.ToArray();

            _userVehicleNumber = await _mindeeService.ExtractDataByEndpointNameAsync(photoBytes, fileName, "vehicle_document");

            if (!string.IsNullOrEmpty(_userVehicleNumber))
            {

                //TODO: move string to resources
                var confirmationMessage = $"Please confirm your details:" +
                    $"\nPassport Number: {_userPassportNumber}" +
                    $"\nVehicle Identification Number: {_userVehicleNumber}" +
                    $"\nIs this information correct?";

                await botClient.SendTextMessageAsync(chatId, confirmationMessage);

                _currentState = BotState.AwaitingDataConfirmation;
            }
            else
            {
                await SendPhotoIncorrectAsync(botClient, chatId, BotState.AwaitingVehicleNumber);
            }
        }
        else

           await SendPhotoIncorrectAsync(botClient, chatId, BotState.AwaitingVehicleNumber);
    } 

    private async Task CreateInvoiceAsync(ITelegramBotClient botClient, long chatId, Message message)
    {
        if (await _openAIService.EnsureSentenceIsPositive(message.Text!))
        {
            //TODO: move string to resources
            await botClient.SendTextMessageAsync(chatId,
                "The fixed price for the insurance is 100 USD. Do you agree?");
            _currentState = BotState.AwaitingPriceConfirmation;
        }
        else
        {
            _currentState = BotState.AwaitingPassportNumber;
            _userPassportNumber = null;
            _userVehicleNumber = null;
            //TODO: move string to resources
            await botClient.SendTextMessageAsync(chatId,
                "Let's try again.");
            await RequestUserPassport(botClient, message.Chat.Id);
        }
    }

    private async Task GeneraeleInsuranseAsync(ITelegramBotClient botClient, long chatId, Message message)
    {
        if (await _openAIService.EnsureSentenceIsPositive(message.Text!))
        {
            var insuranse = await _openAIService.GenerateCanInsurance(_userPassportNumber!, _userVehicleNumber!);

            using (var pdfAsStream = _documentService.GenerateDocument(insuranse))

            await botClient.SendDocumentAsync(chatId, new InputFileStream(pdfAsStream, "CarInsuranse.pdf"));
            await botClient.SendTextMessageAsync(chatId,
                "Thank you for using AutoPalBot! If you'd like to get one more insurance, just message me anything :)");

            _currentState = BotState.StartingConversation;
        }
        else
        {
            //TODO: move string to resources
            await botClient.SendTextMessageAsync(chatId, "We apologize, but the price is fixed at 100 USD.");

            await RequestUserPassport(botClient, message.Chat.Id);
        }
    }

    private async Task SendPhotoIncorrectAsync(ITelegramBotClient botClient, long chatId, BotState state)
    {
        await botClient.SendTextMessageAsync(chatId,
                "Unfortunately, the document you provided is not correct. Let`s try again. " +
                "Please, send a photo of your document again.");

        _currentState = state;
    }
}