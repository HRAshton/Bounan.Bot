using Bounan.Bot.BusinessLogic.CommandDto;
using Bounan.Bot.BusinessLogic.Handlers.MessageHandlers;
using Bounan.Bot.TelegramBot.Telegram;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;

namespace Bounan.Bot.BusinessLogic.Handlers.CallbackQueryHandlers;

public class WatchCallbackQueryHandler(
    ILogger<WatchCallbackQueryHandler> logger,
    WatchMessageHandler watchMessageHandler) : ICallbackQueryHandler
{
    public static bool CanHandle(CallbackQuery item) => item.Data?.StartsWith(WatchCommandDto.Command) ?? false;

    public async Task<AnswerCallbackQueryRequest> HandleAsync(
        CallbackQuery updateItem,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling watch callback query {Data}", updateItem.Data);

        ArgumentNullException.ThrowIfNull(updateItem);
        ArgumentNullException.ThrowIfNull(updateItem.Data);
        ArgumentNullException.ThrowIfNull(updateItem.Message);
        ArgumentNullException.ThrowIfNull(updateItem.Message.Chat);

        await watchMessageHandler.HandleAsync(
            new Message
            {
                Chat = updateItem.Message.Chat,
                Text = updateItem.Data,
            },
            cancellationToken);

        return new AnswerCallbackQueryRequest(updateItem.Id);
    }
}