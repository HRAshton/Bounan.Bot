using Bounan.Bot.BusinessLogic.Clients.Shikimori.Models;
using Bounan.Bot.BusinessLogic.CommandDto;
using Bounan.Bot.BusinessLogic.Configs;
using Bounan.Common.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bounan.Bot.BusinessLogic.Helpers;

public static class TelegramHelpers
{
    public static string GetVideoDescription(AnimeInfo? animeInfo, IVideoKey videoKey)
    {
        var animeInfoStr = animeInfo is null
            ? string.Empty
            : animeInfo.Russian ?? animeInfo.Name;
        var episodeStr = animeInfo?.Episodes is null or 1
            ? string.Empty
            : $"Серия {videoKey.Episode}";
        var dubStr = videoKey.Dub.Replace(".", string.Empty); // Prevent parsing as url

        return $"<b>{animeInfoStr}</b>\n{episodeStr}\nОзвучка: <i>{dubStr}</i>";
    }

    public static InlineKeyboardMarkup GetKeyboard(
        IVideoKey currentVideo,
        IEnumerable<int> allEpisodes,
        ButtonsPagination pagingConfig)
    {
        var episodesPerPage = allEpisodes
            .Distinct()
            .OrderBy(ep => ep)
            .Select((ep, i) => (ep, i))
            .GroupBy(x => x.i / ((pagingConfig.Columns * pagingConfig.Rows) - 2))
            .Select(x => x.Select(y => y.ep))
            .ToArray();

        var currentPageIndex = Array.IndexOf(
            episodesPerPage,
            episodesPerPage.Single(p => p.Contains(currentVideo.Episode)));
        var isFirstPage = currentPageIndex == 0;
        var isLastPage = currentPageIndex == episodesPerPage.Length - 1;

        var buttonsToDisplay = episodesPerPage[currentPageIndex].ToList();
        if (!isFirstPage)
        {
            var lastEpOnPrevPage = episodesPerPage[currentPageIndex - 1].Last();
            buttonsToDisplay.Insert(0, lastEpOnPrevPage);
        }

        if (!isLastPage)
        {
            var firstEpOnNextPage = episodesPerPage[currentPageIndex + 1].First();
            buttonsToDisplay.Add(firstEpOnNextPage);
        }

        // If there are less than 2 episodes, don't display any episode buttons
        var rows = buttonsToDisplay.Count < 2
            ? []
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
                .Select(x => x.Select(group => group.btn).ToArray())
                .ToList();

        if (!isFirstPage)
        {
            rows[0][0].Text = "<<";
        }

        if (!isLastPage)
        {
            rows[^1][^1].Text = ">>";
        }

        rows.Add([
            new InlineKeyboardButton("🔍 О релизе")
            {
                CallbackData = CommandConvert.SerializeCommand(
                    new InfoCommandDto { MyAnimeListId = currentVideo.MyAnimeListId }),
            }
        ]);

        return new InlineKeyboardMarkup(rows);
    }
}