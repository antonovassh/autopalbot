using AutoPalBot.Models.OpenAI;
using AutoPalBot.Models.User;
using AutoPalBot.Repositories.Users;
using AutoPalBot.Services.DocumentGenerator;
using AutoPalBot.Services.Mindee;
using AutoPalBot.Services.OpenAI;
using System.Reflection.Metadata.Ecma335;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using Message = Telegram.Bot.Types.Message;

namespace AutoPalBot.Services.Bot;

public class BotService : IBotService
{
    private readonly IOpenAIService _openAIService;
    private readonly IDocumentService _documentService;
    private readonly IMindeeService _mindeeService;
    private readonly IUsersRepository _usersRepository;

    public BotService(
        IOpenAIService openAISerice, 
        IDocumentService documentService,
        IMindeeService mindeeService,
        IUsersRepository usersRepository)
    {
        _openAIService = openAISerice;
        _documentService = documentService;
        _mindeeService = mindeeService;
        _usersRepository = usersRepository;
    }

    public async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken)
    {
        if (update.Message is not { } message)
            return;

        var user = _usersRepository.GetUser(message.Chat.Id);
        user ??= new UserModel
            {
                BotState = BotState.StartingConversation
            }; 

        switch (user.BotState)
        {
            case BotState.StartingConversation:
                user = await RequestUserPassport(botClient, message.Chat.Id, user);
                break;

            case BotState.AwaitingPassportNumber:
                user = await ProcessUserMessage(
                    botClient,
                    message.Chat.Id,
                    message,
                    user,
                    () => ParsePassportPhotoAsync(botClient, message.Chat.Id, message, user),
                    async () => {
                        await botClient.SendTextMessageAsync(
                            message.Chat.Id,
                            "Please, send me a photo of your passport.");
                    },
                    () => Task.FromResult(message.Photo != null));
                break;

            case BotState.AwaitingVehicleNumber:
                user = await ProcessUserMessage(
                    botClient,
                    message.Chat.Id,
                    message,
                    user,
                    () => ParseVehicleIdentificationPhotoAsync(botClient, message.Chat.Id, message, user),
                    async () => {
                        await botClient.SendTextMessageAsync(
                            message.Chat.Id,
                            "Please, send a photo of your vehicle identification document.");
                    },
                    () => Task.FromResult(message.Photo != null));
                break;

            case BotState.AwaitingDataConfirmation:
                user = await ProcessUserMessage(
                    botClient,
                    message.Chat.Id,
                    message,
                    user,
                    () => CreateInvoiceAsync(botClient, message.Chat.Id, message, user),
                    async () => {
                        await botClient.SendTextMessageAsync(
                            message.Chat.Id,
                            $"Please confirm your details:" +
                            $"\nPassport Number: {user.PassportNumber}" +
                            $"\nVehicle Identification Number: {user.VehicleNumber}" +
                            $"\nIs this information correct?");
                    },
                    async () => {
                        if (message.Text == null)
                            return false;

                        if (message.Text.Last() == '?')
                            return false;

                        var isQuestion = await _openAIService.EnsureSentenceIsQuestion(message.Text!);

                        return !isQuestion;
                    });
                break;

            case BotState.AwaitingPriceConfirmation:
                user = await ProcessUserMessage(
                    botClient,
                    message.Chat.Id,
                    message,
                    user,
                    () => GeneraeleInsuranseAsync(botClient, message.Chat.Id, message, user),
                    async () => {
                        await botClient.SendTextMessageAsync(
                            message.Chat.Id, 
                            "The fixed price for the insurance is 100 USD. Do you agree?");
                    },
                    async () => {
                        if (message.Text == null)
                            return false;

                        if (message.Text.Last() == '?')
                            return false;

                        var isQuestion = await _openAIService.EnsureSentenceIsQuestion(message.Text!);

                        return !isQuestion;
                    });
                break;

            case BotState.AwaitingForQuestion:
                user = await AnswerFromUserQuestion(botClient, message.Chat.Id, message, user);
                break;
        }

        _usersRepository.SetUser(message.Chat.Id, user);
    }

  

    private async Task<UserModel> ProcessUserMessage(
        ITelegramBotClient botClient,
        long chatId,
        Message message,
        UserModel user,
        Func<Task<UserModel>> onSeccess,
        Func<Task> onFail,
        Func<Task<bool>> predicate)
    {
        if (await predicate())
        {
            return await onSeccess();
        }
        else if (message.Text != null)
        {
            var aiResponse = await _openAIService.AnswerAsInsuranceAgent(message.Text);

            await botClient.SendTextMessageAsync(chatId, aiResponse);
        }
        await onFail();

        return user;
    }

    private async Task<UserModel> AnswerFromUserQuestion(ITelegramBotClient botClient, long chatId, Message message, UserModel user)
    {

        if (message.Text == null)
        {
            await botClient.SendTextMessageAsync(chatId, "Please, send me your question or /restart to restart");
        }
        else if (message.Text == "/restart")
        {
            user = await RequestUserPassport(botClient, message.Chat.Id, user);
        }
        else
        {
            var aiResponse = await _openAIService.AnswerAsInsuranceAgent(message.Text!);

            await botClient.SendTextMessageAsync(chatId, aiResponse);

            await botClient.SendTextMessageAsync(chatId, "Please, send me your question or /restart to restart");
        }

        return user;
    }

    private async Task<UserModel> RequestUserPassport(ITelegramBotClient botClient, long chatId, UserModel user)
    {
        Chat chat = await botClient.GetChatAsync(chatId);
        string username = chat.Username!;

        //TODO: move string to resources
        await botClient.SendTextMessageAsync(chatId,
               $"Hello {username}! I'm your virtual assistant from InsuranceCompany. " +
               "My name is AutoPalBot.I can help you quickly and easily get car insurance. " +
               "Just share a photo of your passport and vehicle ID document, " +
               "and I'll take care of the rest.Let's get started on securing your vehicle today!" +
               " Please, send me a photo of your passport.");

        user.BotState = BotState.AwaitingPassportNumber;

        return user;
    }

    private async Task<UserModel> ParsePassportPhotoAsync(ITelegramBotClient botClient, long chatId, Message message, UserModel user)
    {
        if (message.Photo != null)
        {
            string fileName = $"{message.Chat.Username}_passport.jpg";

            var photo = message.Photo[^1]; // Get the last (largest) photo
            var photoFile = await botClient.GetFileAsync(photo.FileId);
            var photoStream = new MemoryStream();

            await botClient.DownloadFileAsync(photoFile.FilePath!, photoStream);

            byte[] photoBytes = photoStream.ToArray();

            var parsedValue = await _mindeeService.ExtractDataByEndpointNameAsync(photoBytes, fileName, "passport");

            if (!string.IsNullOrEmpty(parsedValue))
            {
                user.BotState = BotState.AwaitingVehicleNumber;
                user.PassportNumber = parsedValue;

                //TODO: move string to resources
                await botClient.SendTextMessageAsync(chatId,
                    "Thank you! Now, please send a photo of your vehicle identification document.");
            }
            else
            {
                await SendPhotoIncorrectAsync(botClient, chatId);
                user.BotState = BotState.AwaitingPassportNumber;
            }
        }
        else
        {
            await SendPhotoIncorrectAsync(botClient, chatId);
            user.BotState = BotState.AwaitingPassportNumber;
        }

        return user;
    }

    private async Task<UserModel> ParseVehicleIdentificationPhotoAsync(ITelegramBotClient botClient, long chatId, Message message, UserModel user)
    {
        if (message.Photo != null)
        {
            string fileName = $"{message.Chat.Username}_car_license.jpg";

            var photo = message.Photo[^1]; // Get the last (largest) photo
            var photoFile = await botClient.GetFileAsync(photo.FileId);
            var photoStream = new MemoryStream();

            await botClient.DownloadFileAsync(photoFile.FilePath!, photoStream);

            byte[] photoBytes = photoStream.ToArray();

            var parsedValue = await _mindeeService.ExtractDataByEndpointNameAsync(photoBytes, fileName, "vehicle_document");

            if (!string.IsNullOrEmpty(parsedValue))
            {
                user.BotState = BotState.AwaitingDataConfirmation;
                user.VehicleNumber = parsedValue;

                //TODO: move string to resources
                var confirmationMessage = $"Please confirm your details:" +
                    $"\nPassport Number: {user.PassportNumber}" +
                    $"\nVehicle Identification Number: {user.VehicleNumber}" +
                    $"\nIs this information correct?";

                await botClient.SendTextMessageAsync(chatId, confirmationMessage);
            }
            else
            {
                await SendPhotoIncorrectAsync(botClient, chatId);
                user.BotState = BotState.AwaitingVehicleNumber;
            }
        }
        else
        {
            await SendPhotoIncorrectAsync(botClient, chatId);
            user.BotState = BotState.AwaitingVehicleNumber;
        }

        return user;
    } 

    private async Task<UserModel> CreateInvoiceAsync(ITelegramBotClient botClient, long chatId, Message message, UserModel user)
    {
        if (await _openAIService.EnsureSentenceIsPositive(message.Text!))
        {
            //TODO: move string to resources
            await botClient.SendTextMessageAsync(chatId,
                "The fixed price for the insurance is 100 USD. Do you agree?");
            user.BotState = BotState.AwaitingPriceConfirmation;
        }
        else
        {
            user.BotState = BotState.AwaitingPassportNumber;
            user.PassportNumber = null;
            user.VehicleNumber = null;

            //TODO: move string to resources
            await botClient.SendTextMessageAsync(chatId,
                "Let's try again.");
            await RequestUserPassport(botClient, message.Chat.Id, user);
        }

        return user;
    }

    private async Task<UserModel> GeneraeleInsuranseAsync(ITelegramBotClient botClient, long chatId, Message message, UserModel user)
    {
        if (await _openAIService.EnsureSentenceIsPositive(message.Text!))
        {
            var insuranse = await _openAIService.GenerateCanInsurance(user.PassportNumber!, user.VehicleNumber!);

            using (var pdfAsStream = _documentService.GenerateDocument(insuranse))

            await botClient.SendDocumentAsync(chatId, new InputFileStream(pdfAsStream, "CarInsuranse.pdf"));

            user.BotState = BotState.AwaitingForQuestion;
        }
        else
        {
            //TODO: move string to resources
            await botClient.SendTextMessageAsync(chatId, "We apologize, but the price is fixed at 100 USD.");
            user.BotState = BotState.AwaitingForQuestion;
        }

        await botClient.SendTextMessageAsync(
            chatId,
            "Thank you for using AutoPalBot! If you have any questions feel free to ask or send me /restart to restart:)");

        return user;
    }

    private async Task SendPhotoIncorrectAsync(ITelegramBotClient botClient, long chatId)
    {
        await botClient.SendTextMessageAsync(chatId,
                "Unfortunately, the document you provided is not correct. Let`s try again. " +
                "Please, send a photo of your document again.");
    }
}