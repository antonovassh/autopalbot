using System;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

class Program
{
    private static ITelegramBotClient botClient;

    static async Task Main(string[] args)
    {
        botClient = new TelegramBotClient("6984681761:AAHSmqsAG-twvGG5GtX8loYCBIPpcos20LU");

        var me = await botClient.GetMeAsync();
        Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");

        using var cts = new System.Threading.CancellationTokenSource();

        // Start receiving updates
        botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            },
            cancellationToken: cts.Token
        );

        Console.WriteLine("Press any key to exit");
        Console.ReadKey();

        // Send cancellation request to stop bot
        cts.Cancel();
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, System.Threading.CancellationToken cancellationToken)
    {
        // Only process Message updates
        if (update.Type != UpdateType.Message)
            return;

        // Only process text messages
        var message = update.Message;
        if (message.Type != MessageType.Text)
            return;

        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Hello! I am your auto insurance bot. I can help you purchase auto insurance. Please send me a photo of your passport and ID document.",
            cancellationToken: cancellationToken
        );
    }

    private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, System.Threading.CancellationToken cancellationToken)
    {
        // Log error message
        Console.WriteLine($"An error occurred: {exception.Message}");
        return Task.CompletedTask;
    }
}
