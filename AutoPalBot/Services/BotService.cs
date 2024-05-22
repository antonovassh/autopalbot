using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace AutoPalBot.Services;

public class BotService : IBotService
{
    private enum BotState
    {
        AwaitingPassportNumber,
        AwaitingVehicleNumber,
        ConfirmingDetails,
        AwaitingPriceConfirmation
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
            case BotState.ConfirmingDetails:
                await HandleConfirmationAsync(botClient, chatId, messageText);
                break;
            case BotState.AwaitingPriceConfirmation:
                await HandlePriceConfirmationAsync(botClient, chatId, messageText);
                break;
        }
    }

    private async Task HandlePassportNumberAsync(ITelegramBotClient botClient, long chatId, string messageText)
    {
        _userPassportNumber = messageText;
        _currentState = BotState.AwaitingVehicleNumber;
        await botClient.SendTextMessageAsync(chatId, "Thank you! Now, please enter your vehicle identification number.");
    }

    private async Task HandleVehicleNumberAsync(ITelegramBotClient botClient, long chatId, string messageText)
    {
        _userVehicleNumber = messageText;
        _currentState = BotState.ConfirmingDetails;

        var confirmationMessage = $"Please confirm your details:\nPassport Number: {_userPassportNumber}\nVehicle Identification Number: {_userVehicleNumber}\nIs this information correct? (yes/no)";
        await botClient.SendTextMessageAsync(chatId, confirmationMessage);
    }

    private async Task HandleConfirmationAsync(ITelegramBotClient botClient, long chatId, string messageText)
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

    private async Task HandlePriceConfirmationAsync(ITelegramBotClient botClient, long chatId, string messageText)
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
}


