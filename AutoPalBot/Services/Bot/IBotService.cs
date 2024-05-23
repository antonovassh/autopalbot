using Telegram.Bot.Types;
using Telegram.Bot;

namespace AutoPalBot.Services.Bot;

public interface IBotService
{
    Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken = default);
}
