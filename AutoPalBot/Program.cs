using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.DependencyInjection;
using AutoPalBot.Services.Bot;
using AutoPalBot.Services.OpenAI;
using AutoPalBot.Services.DocumentGenerator;
using AutoPalBot.Services.Mindee;

namespace AutoPalBot;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            var services = new ServiceCollection()
               .AddSingleton<IOpenAIService, OpenAIService>()
               .AddSingleton<IBotService, BotService>()
               .AddSingleton<IDocumentService, DocumentService>()
               .AddSingleton<IMindeeService, MindeeService>()
               .BuildServiceProvider();

            var botService = services.GetRequiredService<IBotService>();

            //TODO: telegram token to secrets.
            var botClient = new TelegramBotClient("6984681761:AAHSmqsAG-twvGG5GtX8loYCBIPpcos20LU");

            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            botClient.StartReceiving(
                updateHandler: (client, update, token) => HandleUpdateAsync(client, update, botService, token),
                pollingErrorHandler: HandleErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await botClient.GetMeAsync();
            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            cts.Cancel();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient client, Update update, IBotService botService, CancellationToken token)
    {
        await botService.HandleUpdateAsync(client, update, token);
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