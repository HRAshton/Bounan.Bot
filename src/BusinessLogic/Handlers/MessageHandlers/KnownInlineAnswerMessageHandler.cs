using Bounan.Bot.BusinessLogic.Constants;
using Bounan.Bot.TelegramBot.Telegram;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bounan.Bot.BusinessLogic.Handlers.MessageHandlers;

public class KnownInlineAnswerMessageHandler(
    ILogger<KnownInlineAnswerMessageHandler> logger,
    ITelegramBotClient botClient) : IMessageHandler
{
    public static bool CanHandle(Message item) => item.Text is
        KnownInlineAnswers.AnimeUnavailable or KnownInlineAnswers.NoResults or KnownInlineAnswers.NoRelatedAnime;

    public Task HandleAsync(Message updateItem, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(updateItem);
        ArgumentNullException.ThrowIfNull(updateItem.Text);

        logger.LogInformation("Handling known inline answer {Text}", updateItem.Text);

        return botClient.DeleteMessageAsync(
            updateItem.Chat.Id,
            updateItem.MessageId,
            cancellationToken: cancellationToken);
    }
}