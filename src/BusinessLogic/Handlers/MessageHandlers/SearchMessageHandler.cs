using Bounan.Bot.BusinessLogic.Clients.Shikimori;
using Bounan.Bot.BusinessLogic.CommandDto;
using Bounan.Bot.BusinessLogic.Helpers;
using Bounan.Bot.TelegramBot.Telegram;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bounan.Bot.BusinessLogic.Handlers.MessageHandlers;

public class SearchMessageHandler(
    ILogger<SearchMessageHandler> logger,
    ITelegramBotClient botClient,
    IShikimoriApi shikimoriApi) : IMessageHandler
{
    public static bool CanHandle(Message item) => throw new NotSupportedException();

    public async Task HandleAsync(Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        logger.LogInformation("Searching anime: {Text}", message.Text);

        ArgumentException.ThrowIfNullOrWhiteSpace(message.Text, nameof(message.Text));

        var searchResults = await shikimoriApi.SearchAnimeAsync(message.Text, CancellationToken.None);
        if (searchResults is null or { Length: 0 })
        {
            logger.LogInformation("No search results");
            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                "Ничего не найдено",
                cancellationToken: cancellationToken);
            return;
        }

        logger.LogInformation("Search {Text} returned {Count} results", message.Text, searchResults.Length);
        var buttons = searchResults
            .Select(anime => new[]
            {
                new InlineKeyboardButton(anime.Russian ?? anime.Name)
                {
                    CallbackData = CommandConvert.SerializeCommand(new InfoCommandDto { MyAnimeListId = anime.Id }),
                },
            });

        logger.LogInformation("Sending {Count} search results", searchResults.Length);
        await botClient.SendTextMessageAsync(
            message.Chat.Id,
            "Наиболее подходящие результаты:",
            replyMarkup: new InlineKeyboardMarkup(buttons),
            cancellationToken: cancellationToken);
    }
}