using Bounan.Bot.BusinessLogic.CommandDto;
using Bounan.Bot.BusinessLogic.Constants;
using Bounan.Bot.BusinessLogic.Helpers;
using Bounan.Bot.TelegramBot.Telegram;
using Bounan.LoanApi.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;

namespace Bounan.Bot.BusinessLogic.Handlers.InlineQueryHandlers;

public class DubsInlineQueryHandler(
    ILogger<DubsInlineQueryHandler> logger,
    ILoanApiComClient botLoanApiClient) : IInlineQueryHandler
{
    public static bool CanHandle(InlineQuery item) => item.Query.StartsWith(DubsCommandDto.Command);

    public async Task<AnswerInlineQueryRequest> HandleAsync(
        InlineQuery inlineQuery,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling dubs inline query {Query}", inlineQuery.Query);

        var commandDto = CommandConvert.DeserializeCommand<DubsCommandDto>(inlineQuery.Query);
        if (commandDto is null)
        {
            logger.LogWarning("Failed to deserialize command {Command}", inlineQuery.Query);
            return TelegramInlineItemsHelpers.EmptyResults(inlineQuery.Id);
        }

        var searchResults = await botLoanApiClient.SearchAsync(commandDto.MyAnimeListId, cancellationToken);
        if (searchResults is null or { Count: 0 })
        {
            logger.LogInformation("No dubs for {MyAnimeListId}", commandDto.MyAnimeListId);
            return TelegramInlineItemsHelpers.ConstResult(inlineQuery.Id, KnownInlineAnswers.AnimeUnavailable);
        }

        var uniqueDubs = searchResults
            .OrderBy(item => item.Dub)
            .ThenBy(item => item.Episode)
            .DistinctBy(item => item.Dub);
        logger.LogInformation("Got dubs for {MyAnimeListId}: {Dubs}", commandDto.MyAnimeListId, uniqueDubs);

        var results = uniqueDubs
            .Select(item =>
            {
                var watchCommandDto = new WatchCommandDto
                {
                    MyAnimeListId = commandDto.MyAnimeListId,
                    Dub = AnimeHelpers.DubToKey(item.Dub),
                    Episode = item.Episode,
                };

                var serializedCommand = CommandConvert.SerializeCommand(watchCommandDto);

                return new InlineQueryResultArticle(
                    item.Dub,
                    item.Dub,
                    new InputTextMessageContent(serializedCommand));
            })
            .ToArray();

        logger.LogInformation("Returning {Count} dubs for {MyAnimeListId}", results.Length, commandDto.MyAnimeListId);
        return new AnswerInlineQueryRequest(inlineQuery.Id, results);
    }
}