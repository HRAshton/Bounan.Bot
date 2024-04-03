using Bounan.Bot.BusinessLogic.Clients.Shikimori;
using Bounan.Bot.BusinessLogic.Clients.Shikimori.Models;
using Bounan.Bot.BusinessLogic.CommandDto;
using Bounan.Bot.BusinessLogic.Configs;
using Bounan.Bot.BusinessLogic.Constants;
using Bounan.Bot.BusinessLogic.Extensions;
using Bounan.Bot.BusinessLogic.Helpers;
using Bounan.Bot.TelegramBot.Telegram;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;

namespace Bounan.Bot.BusinessLogic.Handlers.InlineQueryHandlers;

public class RelatedInlineQueryHandler(
    ILogger<RelatedInlineQueryHandler> logger,
    IShikimoriApi shikimoriApi,
    IOptions<ShikimoriConfig> shikimoriConfig) : IInlineQueryHandler
{
    public static bool CanHandle(InlineQuery item) => item.Query.StartsWith(RelatedCommandDto.Command);

    public async Task<AnswerInlineQueryRequest> HandleAsync(
        InlineQuery inlineQuery,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling related inline query {Query}", inlineQuery.Query);

        var commandDto = CommandConvert.DeserializeCommand<RelatedCommandDto>(inlineQuery.Query);
        if (commandDto is null)
        {
            logger.LogWarning("Failed to deserialize command {Command}", inlineQuery.Query);
            return new AnswerInlineQueryRequest(inlineQuery.Id, []);
        }

        RelatedItem[] related;
        try
        {
            related = await shikimoriApi.GetRelatedAnimeAsync(commandDto.MyAnimeListId, cancellationToken);
            logger.LogInformation("Got related anime for {MyAnimeListId}", commandDto.MyAnimeListId);
        }
        catch (Exception)
        {
            related = Array.Empty<RelatedItem>();
            logger.LogWarning("Failed to get related anime for {MyAnimeListId}", commandDto.MyAnimeListId);
        }

        var relatedAnime = related
            .Where(rel => rel.Anime is not null)
            .ToArray();

        if (relatedAnime is null or { Length: 0 })
        {
            logger.LogInformation("No related anime for {MyAnimeListId}", commandDto.MyAnimeListId);
            return new AnswerInlineQueryRequest(
                inlineQuery.Id,
                new[]
                {
                    new InlineQueryResultArticle(
                        KnownInlineAnswers.NoRelatedAnime,
                        KnownInlineAnswers.NoRelatedAnime,
                        new InputTextMessageContent(KnownInlineAnswers.NoRelatedAnime)),
                });
        }

        var results = relatedAnime
            .OrderByDescending(rel => rel.Anime!.Id)
            .Select(anime => TelegramInlineItemsHelpers.AnimeInfo(
                anime.Anime!,
                shikimoriConfig.Value.BaseUrl,
                anime.RelationRussian.Or(anime.Relation)));

        logger.LogInformation(
            "Returning {Count} related anime for {MyAnimeListId}",
            relatedAnime.Length,
            commandDto.MyAnimeListId);
        return new AnswerInlineQueryRequest(inlineQuery.Id, results);
    }
}