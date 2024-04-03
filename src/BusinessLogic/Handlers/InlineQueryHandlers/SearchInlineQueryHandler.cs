using Bounan.Bot.BusinessLogic.Clients.Shikimori;
using Bounan.Bot.BusinessLogic.Configs;
using Bounan.Bot.BusinessLogic.Constants;
using Bounan.Bot.BusinessLogic.Helpers;
using Bounan.Bot.TelegramBot.Telegram;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;

namespace Bounan.Bot.BusinessLogic.Handlers.InlineQueryHandlers;

public class SearchInlineQueryHandler(
    ILogger<SearchInlineQueryHandler> logger,
    IShikimoriApi shikimoriApi,
    IOptions<ShikimoriConfig> shikimoriConfig) : IInlineQueryHandler
{
    public static bool CanHandle(InlineQuery item) => throw new NotSupportedException();

    public async Task<AnswerInlineQueryRequest> HandleAsync(
        InlineQuery inlineQuery,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling search inline query {Query}", inlineQuery.Query);

        var query = inlineQuery.Query;
        if (string.IsNullOrWhiteSpace(query))
        {
            logger.LogInformation("Empty query");
            return TelegramInlineItemsHelpers.EmptyResults(inlineQuery.Id);
        }

        var searchResults = await shikimoriApi.SearchAnimeAsync(query, cancellationToken);
        if (searchResults.Length == 0)
        {
            logger.LogInformation("No results for {Query}", query);
            return TelegramInlineItemsHelpers.ConstResult(inlineQuery.Id, KnownInlineAnswers.NoResults);
        }

        var shikimoriBaseDomain = shikimoriConfig.Value.BaseUrl;
        var results = searchResults
            .Select(anime => TelegramInlineItemsHelpers.AnimeInfo(anime, shikimoriBaseDomain));

        logger.LogInformation("Got {Count} results for {Query}", searchResults.Length, query);
        return new AnswerInlineQueryRequest(inlineQuery.Id, results);
    }
}