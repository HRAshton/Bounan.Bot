using Bounan.Bot.BusinessLogic.Clients.Shikimori;
using Bounan.Bot.BusinessLogic.Clients.Shikimori.Models;
using Bounan.Bot.BusinessLogic.Configs;
using Bounan.Bot.BusinessLogic.Helpers;
using Bounan.Bot.BusinessLogic.Interfaces;
using Bounan.Bot.BusinessLogic.Models;
using Bounan.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bounan.Bot.BusinessLogic.Services;

internal class NotificationService : INotificationService
{
    private readonly TelegramBotConfig _telegramBotConfig;

    public NotificationService(
        ILogger<NotificationService> logger,
        ITelegramBotClient botClient,
        IShikimoriApi shikimoriApi,
        IOptions<TelegramBotConfig> telegramBotConfig)
    {
        Logger = logger;
        BotClient = botClient;
        ShikimoriApi = shikimoriApi;
        _telegramBotConfig = telegramBotConfig.Value;
    }

    private ILogger<NotificationService> Logger { get; }

    private ITelegramBotClient BotClient { get; }

    private IShikimoriApi ShikimoriApi { get; }

    public async Task HandleAsync(BotNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        Logger.LogInformation("Handling notification from AniMan: {Notification}", notification);

        var animeInfo = await ShikimoriApi.GetAnimeInfoAsync(notification.MyAnimeListId);
        ArgumentNullException.ThrowIfNull(animeInfo);
        Logger.LogInformation("Got anime info: {AnimeInfo}", animeInfo.Name);

        if (notification.FileId is null)
        {
            Logger.LogInformation("Notification has no file id, sending failed notification");
            await SendFailedNotificationAsync(notification, animeInfo);
            return;
        }

        Logger.LogInformation("Sending notification with file id: {FileId}", notification.FileId);
        await SendNotificationAsync(notification, animeInfo);
    }

    private async Task SendFailedNotificationAsync(IBotNotification notification, AnimeInfo animeInfo)
    {
        var message = "Не удалось найти видео. Команда уже оповещена.\n"
                      + TelegramHelpers.GetVideoDescription(animeInfo, notification.Episode);
        foreach (var chatId in notification.ChatIds)
        {
            await BotClient.SendTextMessageAsync(chatId, message, parseMode: ParseMode.MarkdownV2);
        }
    }

    private async Task SendNotificationAsync(IBotNotification notification, AnimeInfo animeInfo)
    {
        ArgumentNullException.ThrowIfNull(notification.ChatIds);
        ArgumentNullException.ThrowIfNull(notification.FileId);

        var allEpisodes = animeInfo.Episodes.HasValue
            ? Enumerable.Range(1, animeInfo.Episodes.Value)
            : [notification.Episode];
        var keyboard = TelegramHelpers.GetKeyboard(notification, allEpisodes, _telegramBotConfig.ButtonsPagination);

        var message = TelegramHelpers.GetVideoDescription(animeInfo, notification.Episode);
        foreach (var chatId in notification.ChatIds)
        {
            await BotClient.SendVideoAsync(
                chatId,
                new InputFileId(notification.FileId),
                caption: message,
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: keyboard);
        }
    }
}