using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AutoPalBot.Services;

public class Bot
{
    private enum BotState
    {
        AwaitingPassportNumber,
        AwaitingVehicleNumber,
        ConfirmingDetails,
        AwaitingPriceConfirmation
    }

    private static BotState _currentState = BotState.AwaitingPassportNumber;

    private static string _userPassportNumber;
    private static string _userVehicleNumber;

    public static async Task Main(string[] args)
    {
        var botClient = new TelegramBotClient("6984681761:AAHSmqsAG-twvGG5GtX8loYCBIPpcos20LU");

        using var cts = new CancellationTokenSource();

        // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
        botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            },
            cancellationToken: cts.Token
        );

        var me = await botClient.GetMeAsync();
        Console.WriteLine($"Start listening for @{me.Username}");
        Console.ReadLine();

        // Send cancellation request to stop bot
        cts.Cancel();
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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
            case BotState.ConfirmingDetails:
                await HandleConfirmationAsync(botClient, chatId, messageText);
                break;
            case BotState.AwaitingPriceConfirmation:
                await HandlePriceConfirmationAsync(botClient, chatId, messageText);
                break;
        }
    }

    private static async Task HandlePassportNumberAsync(ITelegramBotClient botClient, long chatId, string messageText)
    {
        _userPassportNumber = messageText;
        _currentState = BotState.AwaitingVehicleNumber;
        await botClient.SendTextMessageAsync(chatId, "Thank you! Now, please enter your vehicle identification number.");
    }

    private static async Task HandleVehicleNumberAsync(ITelegramBotClient botClient, long chatId, string messageText)
    {
        _userVehicleNumber = messageText;
        _currentState = BotState.ConfirmingDetails;

        var confirmationMessage = $"Please confirm your details:\nPassport Number: {_userPassportNumber}\nVehicle Identification Number: {_userVehicleNumber}\nIs this information correct? (yes/no)";
        await botClient.SendTextMessageAsync(chatId, confirmationMessage);
    }

    private static async Task HandleConfirmationAsync(ITelegramBotClient botClient, long chatId, string messageText)
    {
        if (messageText.ToLower() == "yes")
        {
            _currentState = BotState.AwaitingPriceConfirmation;
            await botClient.SendTextMessageAsync(chatId, "The fixed price for the insurance is 100 USD. Do you agree? (yes/no)");
        }
        else if (messageText.ToLower() == "no")
        {
            _currentState = BotState.AwaitingPassportNumber;
            _userPassportNumber = null;
            _userVehicleNumber = null;
            await botClient.SendTextMessageAsync(chatId, "Let's try again. Please enter your passport number.");
        }
    }

    private static async Task HandlePriceConfirmationAsync(ITelegramBotClient botClient, long chatId, string messageText)
    {
        if (messageText.ToLower() == "yes")
        {
            var policyDocument = await OpenAIService.GeneratePolicyDocument(_userPassportNumber);// _userVehicleNumber);
            await botClient.SendTextMessageAsync(chatId, $"Thank you! Here is your insurance policy:\n{policyDocument}");
        }
        else if (messageText.ToLower() == "no")
        {
            await botClient.SendTextMessageAsync(chatId, "We apologize, but the price is fixed at 100 USD.");
        }
    }

    private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
}
