using Bounan.Bot.BusinessLogic.Clients.Shikimori;
using Bounan.Bot.BusinessLogic.Clients.Shikimori.Models;
using Bounan.Bot.BusinessLogic.Configs;
using Bounan.Bot.BusinessLogic.Helpers;
using Bounan.Bot.BusinessLogic.Interfaces;
using Bounan.Common;
using Bounan.LoanApi.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bounan.Bot.BusinessLogic.Services;

internal class NotificationService(
    ILogger<NotificationService> logger,
    ITelegramBotClient botClient,
    IShikimoriApi shikimoriApi,
    IOptions<TelegramBotConfig> telegramBotConfig,
    ILoanApiComClient loanApiComClient,
    IFileIdFinder fileIdFinder)
    : INotificationService
{
    private readonly TelegramBotConfig _telegramBotConfig = telegramBotConfig.Value;

    private ILogger<NotificationService> Logger => logger;

    private ITelegramBotClient BotClient => botClient;

    private IShikimoriApi ShikimoriApi => shikimoriApi;

    private ILoanApiComClient LoanApiComClient => loanApiComClient;

    private IFileIdFinder FileIdFinder => fileIdFinder;

    public async Task HandleAsync(VideoDownloadedNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (notification.SubscriberChatIds is null or { Count: 0 })
        {
            Logger.LogInformation("Notification has no subscribers, skipping");
            return;
        }

        Logger.LogInformation("Handling notification from AniMan: {@Notification}", notification);

        var animeInfo = await ShikimoriApi.GetAnimeInfoAsync(notification.VideoKey.MyAnimeListId);
        ArgumentNullException.ThrowIfNull(animeInfo);
        Logger.LogInformation("Got anime info: {@AnimeInfo}", animeInfo);

        if (notification.MessageId is null)
        {
            Logger.LogInformation("Notification has no file id, sending failed notification");
            await SendFailedNotificationAsync(notification, animeInfo);
            return;
        }

        try
        {
            Logger.LogInformation("Sending notification with message id: {FileId}", notification.MessageId);
            await SendNotificationAsync(notification, animeInfo);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to send notification");
            await SendFailedNotificationAsync(notification, animeInfo);
        }
    }

    private async Task SendFailedNotificationAsync(VideoDownloadedNotification notification, AnimeInfo animeInfo)
    {
        ArgumentNullException.ThrowIfNull(notification.SubscriberChatIds);
        var message = "Не удалось найти видео. Команда уже оповещена.\n"
                      + TelegramHelpers.GetVideoDescription(animeInfo, notification.VideoKey, scenes: null);
        foreach (var chatId in notification.SubscriberChatIds)
        {
            await BotClient.SendTextMessageAsync(chatId, message, parseMode: ParseMode.Html);
        }
    }

    private async Task SendNotificationAsync(VideoDownloadedNotification notification, AnimeInfo animeInfo)
    {
        ArgumentNullException.ThrowIfNull(notification.SubscriberChatIds);
        ArgumentNullException.ThrowIfNull(notification.MessageId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(notification.MessageId.Value);

        var fileId = await FileIdFinder.GetFileIdAsync(notification.MessageId.Value);
        ArgumentNullException.ThrowIfNull(fileId);

        var searchResults = await LoanApiComClient.GetExistingVideos(animeInfo.Id);
        var allEpisodes = searchResults.Select(x => x.Episode);
        var keyboard = TelegramHelpers.GetKeyboard(
            notification.VideoKey,
            allEpisodes,
            notification.PublishingDetails,
            _telegramBotConfig);

        var message = TelegramHelpers.GetVideoDescription(animeInfo, notification.VideoKey, notification.Scenes);
        foreach (var chatId in notification.SubscriberChatIds)
        {
            await BotClient.SendVideoAsync(
                chatId,
                new InputFileId(fileId),
                caption: message,
                parseMode: ParseMode.Html,
                replyMarkup: keyboard);
        }
    }
}