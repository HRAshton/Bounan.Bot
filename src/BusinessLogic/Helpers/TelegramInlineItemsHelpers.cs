using Bounan.Bot.BusinessLogic.Clients.Shikimori.Models;
using Bounan.Bot.BusinessLogic.CommandDto;
using Telegram.Bot.Requests;
using Telegram.Bot.Types.InlineQueryResults;

namespace Bounan.Bot.BusinessLogic.Helpers;

public static class TelegramInlineItemsHelpers
{
    public static AnswerInlineQueryRequest EmptyResults(string inlineQueryId)
    {
        return new AnswerInlineQueryRequest(inlineQueryId, []);
    }

    public static AnswerInlineQueryRequest ConstResult(string inlineQueryId, string text)
    {
        return new AnswerInlineQueryRequest(
            inlineQueryId,
            [
                new InlineQueryResultArticle(
                    text,
                    text,
                    new InputTextMessageContent(text))
            ]);
    }

    public static InlineQueryResultArticle AnimeInfo(
        AnimeInfo anime,
        Uri shikimoriBaseDomain,
        string? additionalInfo = null)
    {
        var infoCommand = new InfoCommandDto { MyAnimeListId = anime.Id };

        var serializedCommand = CommandConvert.SerializeCommand(infoCommand);

        return new InlineQueryResultArticle(
            anime.Id.ToString(),
            string.IsNullOrWhiteSpace(anime.Russian) ? anime.Name : anime.Russian,
            new InputTextMessageContent(serializedCommand))
        {
            Description = anime.AiredOn?[..4] +
                          (string.IsNullOrWhiteSpace(additionalInfo) ? string.Empty : $", {additionalInfo}"),
            ThumbnailUrl = new Uri(shikimoriBaseDomain, anime.Image.Preview).ToString(),
        };
    }
}