using Bounan.Bot.BusinessLogic.Clients.Shikimori;
using Bounan.Bot.BusinessLogic.Clients.Shikimori.Models;
using Bounan.Bot.BusinessLogic.CommandDto;
using Bounan.Bot.BusinessLogic.Configs;
using Bounan.Bot.BusinessLogic.Helpers;
using Bounan.Bot.BusinessLogic.Interfaces;
using Bounan.Bot.BusinessLogic.Models;
using Bounan.Bot.BusinessLogic.Services;
using Bounan.Bot.TelegramBot.Telegram;
using Bounan.Common.Enums;
using Bounan.Common.Models;
using Bounan.LoanApi.Interfaces;
using Bounan.LoanApi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bounan.Bot.BusinessLogic.Handlers.MessageHandlers;

public class WatchMessageHandler : IMessageHandler
{
    private readonly TelegramBotConfig _telegramBotConfig;

    public WatchMessageHandler(
        ILogger<WatchMessageHandler> logger,
        IShikimoriApi shikimoriApi,
        ILoanApiComClient botLoanApiComClient,
        IAniManClient aniManClient,
        ITelegramBotClient botClient,
        IOptions<TelegramBotConfig> telegramBotConfig,
        IFileIdFinder fileIdFinder)
    {
        _telegramBotConfig = telegramBotConfig.Value;
        Logger = logger;
        ShikimoriApi = shikimoriApi;
        BotLoanApiComClient = botLoanApiComClient;
        AniManClient = aniManClient;
        BotClient = botClient;
        FileIdFinder = fileIdFinder;
    }

    private ILogger Logger { get; }

    private IShikimoriApi ShikimoriApi { get; }

    private ILoanApiComClient BotLoanApiComClient { get; }

    private IAniManClient AniManClient { get; }

    private ITelegramBotClient BotClient { get; }

    private IFileIdFinder FileIdFinder { get; }

    public static bool CanHandle(Message message) => message.Text?.StartsWith(WatchCommandDto.Command) ?? false;

    public async Task HandleAsync(Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.Text);
        ArgumentNullException.ThrowIfNull(message.Chat.Id);

        var commandDto = CommandConvert.DeserializeCommand<WatchCommandDto>(message.Text);
        if (commandDto is null)
        {
            Logger.LogInformation("Failed to deserialize command {Command}", message.Text);
            return;
        }

        var searchResults = await BotLoanApiComClient.GetExistingVideos(commandDto.MyAnimeListId, cancellationToken);
        if (searchResults is null or { Count: 0 })
        {
            Logger.LogWarning("No videos for {MyAnimeListId}", commandDto.MyAnimeListId);
            await BotClient.SendTextMessageAsync(
                message.Chat.Id,
                "Этого аниме (пока?) нет в базе",
                cancellationToken: cancellationToken);
            return;
        }

        var episodesInDub = searchResults
            .Where(item => AnimeHelpers.DubToKey(item.Dub) == AnimeHelpers.DubToKey(commandDto.Dub))
            .Select(item => item.Episode)
            .OrderBy(ep => ep)
            .ToArray();
        ArgumentOutOfRangeException.ThrowIfZero(episodesInDub.Length);

        var selectedVideo = searchResults
            .FirstOrDefault(v => AnimeHelpers.DubToKey(v.Dub) == AnimeHelpers.DubToKey(commandDto.Dub)
                                 && v.Episode == commandDto.Episode);
        if (selectedVideo is null)
        {
            Logger.LogInformation("Video not found in dub");
            await SendSwitchDubButtonsAsync(message.Chat.Id, searchResults, commandDto.Episode);
            return;
        }

        var botRequest = new BotRequest(
            MyAnimeListId: selectedVideo.MyAnimeListId,
            Dub: selectedVideo.Dub,
            Episode: selectedVideo.Episode,
            ChatId: message.Chat.Id);
        var videoInfo = await AniManClient.GetAnimeAsync(botRequest, CancellationToken.None);

        var keyboard = TelegramHelpers.GetKeyboard(
            commandDto,
            searchResults.Select(v => v.Episode),
            _telegramBotConfig.ButtonsPagination);

        switch (videoInfo?.Status)
        {
            case VideoStatus.Pending:
            case VideoStatus.Downloading:
                Logger.LogInformation("Video not downloaded");
                await BotClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Видео готовится. Я пришлю его, как только будет готово",
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken);
                break;

            case VideoStatus.Failed:
            case VideoStatus.NotAvailable:
                Logger.LogInformation("Video failed to download");
                await BotClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Я не смог найти эту серию. Какая-то ошибка, разработчик уже уведомлен",
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken);
                break;

            case VideoStatus.Downloaded:
                await SendVideoAsync(message, commandDto, videoInfo, keyboard, cancellationToken);
                break;

            case VideoStatus.Unknown:
            case null:
                Logger.LogError("Lambda returned null");
                await BotClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Что-то сломалось. Разработчик уже уведомлен",
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken);
                break;

            default:
                throw new InvalidOperationException($"Unknown video status: {videoInfo.Status}");
        }
    }

    private async Task SendVideoAsync(
        Message message,
        IVideoKey commandDto,
        IBotResponse videoInfo,
        IReplyMarkup keyboard,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(videoInfo.MessageId);

        AnimeInfo? animeInfo;
        try
        {
            animeInfo = await ShikimoriApi.GetAnimeInfoAsync(commandDto.MyAnimeListId, cancellationToken);
            Logger.LogInformation(
                "Got anime info for {MyAnimeListId}: {AnimeInfo}",
                commandDto.MyAnimeListId,
                animeInfo);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to get anime info");
            animeInfo = null;
        }

        var fileId = await FileIdFinder.GetFileIdAsync(videoInfo.MessageId.Value);
        ArgumentNullException.ThrowIfNull(fileId);

        await BotClient.SendVideoAsync(
            message.Chat.Id,
            new InputFileId(fileId),
            caption: TelegramHelpers.GetVideoDescription(animeInfo, commandDto),
            parseMode: ParseMode.Html,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task SendSwitchDubButtonsAsync(long chatId, IEnumerable<IVideoKeyWithLink> searchResults, int episode)
    {
        var inOtherDubs = searchResults
            .Where(item => item.Episode == episode)
            .OrderBy(item => item.Dub)
            .ToArray();

        Logger.LogWarning("Episode not found in dub. Other dubs: {Dubs}", inOtherDubs.Select(ep => ep.Dub));

        await BotClient.SendTextMessageAsync(
            chatId,
            "Серии нет в этом переводе. Попробуйте другой",
            replyMarkup: new InlineKeyboardMarkup(
                inOtherDubs.Select(ep => new[]
                {
                    new InlineKeyboardButton(ep.Dub)
                    {
                        CallbackData = CommandConvert.SerializeCommand(
                            new WatchCommandDto
                            {
                                MyAnimeListId = ep.MyAnimeListId,
                                Dub = AnimeHelpers.DubToKey(ep.Dub),
                                Episode = episode,
                            }),
                    },
                })));
    }
}