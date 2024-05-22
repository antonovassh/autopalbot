using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AutoPalBot.Services;
using Microsoft.Extensions.DependencyInjection;

public class Bot
{
    public static async Task Main(string[] args)
    {
        // Настройка служб
        var services = new ServiceCollection()
            .AddSingleton<IBotService, BotService>()
            .BuildServiceProvider();

        var botService = services.GetRequiredService<IBotService>();

        // Инициализация Telegram Bot Client
        var botClient = new TelegramBotClient("6984681761:AAHSmqsAG-twvGG5GtX8loYCBIPpcos20LU");

        using var cts = new CancellationTokenSource();

        // Определение параметров для получения обновлений
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        // Запуск получения обновлений
        botClient.StartReceiving(
            updateHandler: (client, update, token) => HandleUpdateAsync(client, update, token, botService),
            pollingErrorHandler: HandleErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        // Получение информации о боте
        var me = await botClient.GetMeAsync();
        Console.WriteLine($"Start listening for @{me.Username}");
        Console.ReadLine();

        // Отмена получения обновлений
        cts.Cancel();
    }

    // Обработчик обновлений
    private static async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken token, IBotService botService)
    {
        await botService.HandleUpdateAsync(client, update, token);
    }

    // Обработчик ошибок
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

//if (messageText.ToLower() == "/start")
//{
//    await botClient.SendTextMessageAsync(chatId, "Welcome! I am your Car Insurance Bot. Please send a photo of your passport.");
//    return;
//}

//if (message.Photo != null)
//{
//    var fileId = message.Photo.Last().FileId;
//    var file = await botClient.GetFileAsync(fileId);

//    using (var saveImageStream = new FileStream("temp.jpg", FileMode.Create))
//    {
//        await botClient.DownloadFileAsync(file.FilePath, saveImageStream);
//    }

//    var documentData = await MindeeService.ExtractDataAsync("temp.jpg");
//    var documentDataText = JsonConvert.SerializeObject(documentData, Formatting.Indented);

//    await botClient.SendTextMessageAsync(chatId, $"Extracted Data: {documentDataText}\nIs this information correct? (yes/no)");

//    return;
//}


