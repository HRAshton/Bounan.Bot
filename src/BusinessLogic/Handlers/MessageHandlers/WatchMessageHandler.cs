using Bounan.Bot.BusinessLogic.Clients.Shikimori;
using Bounan.Bot.BusinessLogic.Clients.Shikimori.Models;
using Bounan.Bot.BusinessLogic.CommandDto;
using Bounan.Bot.BusinessLogic.Configs;
using Bounan.Bot.BusinessLogic.Helpers;
using Bounan.Bot.BusinessLogic.Interfaces;
using Bounan.Bot.BusinessLogic.Models;
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

public class WatchMessageHandler(
    ILogger<WatchMessageHandler> logger,
    IShikimoriApi shikimoriApi,
    ILoanApiComClient botLoanApiClient,
    IAniManClient aniManClient,
    ITelegramBotClient botClient,
    IOptions<TelegramBotConfig> telegramBotConfig) : IMessageHandler
{
    public static bool CanHandle(Message message) => message.Text?.StartsWith(WatchCommandDto.Command) ?? false;

    public async Task HandleAsync(Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.Text);
        ArgumentNullException.ThrowIfNull(message.Chat.Id);

        var commandDto = CommandConvert.DeserializeCommand<WatchCommandDto>(message.Text);
        if (commandDto is null)
        {
            logger.LogInformation("Failed to deserialize command {Command}", message.Text);
            return;
        }

        var searchResults = await botLoanApiClient.SearchAsync(commandDto.MyAnimeListId, cancellationToken);
        if (searchResults is null or { Count: 0 })
        {
            logger.LogWarning("No videos for {MyAnimeListId}", commandDto.MyAnimeListId);
            await botClient.SendTextMessageAsync(
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
            logger.LogInformation("Video not found in dub");
            await SendSwitchDubButtonsAsync(message.Chat.Id, searchResults, commandDto.Episode);
            return;
        }

        var botRequest = new BotRequest(
            MyAnimeListId: selectedVideo.MyAnimeListId,
            Dub: selectedVideo.Dub,
            Episode: selectedVideo.Episode,
            ChatId: message.Chat.Id);
        var videoInfo = await aniManClient.GetAnimeAsync(botRequest, CancellationToken.None);

        var keyboard = TelegramHelpers.GetKeyboard(
            commandDto,
            searchResults.Select(v => v.Episode),
            telegramBotConfig.Value.ButtonsPagination);

        switch (videoInfo?.Status)
        {
            case VideoStatus.Pending:
            case VideoStatus.Downloading:
                logger.LogInformation("Video not downloaded");
                await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Видео готовится. Я пришлю его, как только будет готово",
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken);
                break;

            case VideoStatus.Failed:
            case VideoStatus.NotAvailable:
                logger.LogInformation("Video failed to download");
                await botClient.SendTextMessageAsync(
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
                logger.LogError("Lambda returned null");
                await botClient.SendTextMessageAsync(
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
        WatchCommandDto commandDto,
        IBotResponse videoInfo,
        IReplyMarkup keyboard,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(videoInfo.FileId);

        AnimeInfo? animeInfo;
        try
        {
            animeInfo = await shikimoriApi.GetAnimeInfoAsync(commandDto.MyAnimeListId, cancellationToken);
            logger.LogInformation(
                "Got anime info for {MyAnimeListId}: {AnimeInfo}",
                commandDto.MyAnimeListId,
                animeInfo);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get anime info");
            animeInfo = null;
        }

        await botClient.SendVideoAsync(
            message.Chat.Id,
            new InputFileId(videoInfo.FileId),
            caption: TelegramHelpers.GetVideoDescription(animeInfo, commandDto.Episode),
            parseMode: ParseMode.MarkdownV2,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task SendSwitchDubButtonsAsync(long chatId, IEnumerable<SearchResultItem> searchResults, int episode)
    {
        var inOtherDubs = searchResults
            .Where(item => item.Episode == episode)
            .OrderBy(item => item.Dub)
            .ToArray();

        logger.LogWarning("Episode not found in dub. Other dubs: {Dubs}", inOtherDubs.Select(ep => ep.Dub));

        await botClient.SendTextMessageAsync(
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