using Bounan.Bot.BusinessLogic.Clients.Shikimori.Models;
using Bounan.Bot.BusinessLogic.CommandDto;
using Bounan.Bot.BusinessLogic.Configs;
using Bounan.Common;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bounan.Bot.BusinessLogic.Helpers;

public static class TelegramHelpers
{
    public static string GetVideoDescription(AnimeInfo? animeInfo, IVideoKey videoKey, Scenes? scenes)
    {
        var animeInfoStr = animeInfo is null
            ? string.Empty
            : $"<b>{animeInfo.Russian ?? animeInfo.Name}</b>";
        var episodeStr = animeInfo?.Episodes is null or 1
            ? string.Empty
            : $"Серия {videoKey.Episode}";
        var dubStr = $"(<i>{videoKey.Dub.Replace(".", string.Empty)}</i>)"; // Prevent parsing as url

        var openingStr = scenes?.Opening is null
            ? string.Empty
            : $"{SecsToStr(scenes.Opening.End)} - Конец опенинга (от {SecsToStr(scenes.Opening.Start)})";

        var endingStr = scenes?.SceneAfterEnding is null
            ? string.Empty
            : $"{SecsToStr(scenes.SceneAfterEnding.Start)} - Сцена-после-титров";

        var videoDescription = string.Join(
            '\n',
            new[]
                {
                    $"{animeInfoStr} {dubStr}",
                    episodeStr,
                    openingStr,
                    endingStr,
                }
                .Where(str => !string.IsNullOrWhiteSpace(str)));

        return videoDescription;
    }

    public static InlineKeyboardMarkup GetKeyboard(
        IVideoKey currentVideo,
        IEnumerable<int> allEpisodes,
        PublishingDetails? publishingDetails,
        TelegramBotConfig telegramBotConfig)
    {
        var pagingConfig = telegramBotConfig.ButtonsPagination;

        var episodesPerPage = allEpisodes
            .Distinct()
            .OrderBy(ep => ep)
            .Select((ep, i) => (ep, i))
            .GroupBy(x => x.i / (pagingConfig.Columns * pagingConfig.Rows))
            .Select(x => x.Select(y => y.ep))
            .ToArray();

        var currentPageIndex = Array.IndexOf(
            episodesPerPage,
            episodesPerPage.Single(p => p.Contains(currentVideo.Episode)));

        var episodeRows = GetEpisodeRows(currentVideo, episodesPerPage, currentPageIndex, pagingConfig);
        var controlRow = GetControlRow(
            currentVideo,
            publishingDetails,
            telegramBotConfig,
            currentPageIndex,
            episodesPerPage);

        var rows = episodeRows.Concat([ controlRow ]);

        return new InlineKeyboardMarkup(rows);
    }

    private static IEnumerable<IEnumerable<InlineKeyboardButton>> GetEpisodeRows(
        IVideoKey currentVideo,
        IEnumerable<int>[] episodesPerPage,
        int currentPageIndex,
        ButtonsPagination pagingConfig)
    {
        var buttonsToDisplay = episodesPerPage[currentPageIndex].ToList();

        // If there are less than 2 episodes, don't display any episode buttons
        var episodeRows = buttonsToDisplay.Count <= 1
            ? [ ]
            : buttonsToDisplay
                .Select(ep => ep == currentVideo.Episode
                    ? new InlineKeyboardButton($"[{ep}]") { Url = "tg://placeholder" }
                    : new InlineKeyboardButton(ep.ToString())
                    {
                        CallbackData = CommandConvert.SerializeCommand(
                            new WatchCommandDto
                            {
                                MyAnimeListId = currentVideo.MyAnimeListId,
                                Dub = AnimeHelpers.DubToKey(currentVideo.Dub),
                                Episode = ep,
                            }),
                    })
                .Select((btn, index) => (btn, index))
                .GroupBy(pair => pair.index / pagingConfig.Columns)
                .Select(x => x.Select(group => group.btn));
        return episodeRows;
    }

    private static List<InlineKeyboardButton> GetControlRow(
        IVideoKey currentVideo,
        PublishingDetails? publishingDetails,
        TelegramBotConfig telegramBotConfig,
        int currentPageIndex,
        IEnumerable<int>[] episodesPerPage)
    {
        var controlRow = new List<InlineKeyboardButton>
        {
            new("🔍 О релизе")
            {
                CallbackData = CommandConvert.SerializeCommand(
                    new InfoCommandDto { MyAnimeListId = currentVideo.MyAnimeListId }),
            },
        };

        if (publishingDetails is not null)
        {
            controlRow.Add(new InlineKeyboardButton("🍿 Все серии")
            {
                Url = string.Join(
                    '/',
                    "https://t.me",
                    telegramBotConfig.PublisherGroupName,
                    publishingDetails.ThreadId,
                    publishingDetails.MessageId),
            });
        }

        var isFirstPage = currentPageIndex == 0;
        var isLastPage = currentPageIndex == episodesPerPage.Length - 1;

        if (!isFirstPage)
        {
            controlRow.Insert(0, new InlineKeyboardButton("<<")
            {
                CallbackData = CommandConvert.SerializeCommand(
                    new WatchCommandDto
                    {
                        MyAnimeListId = currentVideo.MyAnimeListId,
                        Dub = AnimeHelpers.DubToKey(currentVideo.Dub),
                        Episode = episodesPerPage[currentPageIndex - 1].Last(),
                    }),
            });
        }

        if (!isLastPage)
        {
            controlRow.Add(new InlineKeyboardButton(">>")
            {
                CallbackData = CommandConvert.SerializeCommand(
                    new WatchCommandDto
                    {
                        MyAnimeListId = currentVideo.MyAnimeListId,
                        Dub = AnimeHelpers.DubToKey(currentVideo.Dub),
                        Episode = episodesPerPage[currentPageIndex + 1].First(),
                    }),
            });
        }

        return controlRow;
    }

    private static string SecsToStr(float secs)
    {
        var ts = TimeSpan.FromSeconds(secs);
        return $@"{ts:mm\:ss}";
    }
}