using Bounan.Bot.BusinessLogic.CommandDto;
using Bounan.Bot.BusinessLogic.Handlers.MessageHandlers;
using Bounan.Bot.TelegramBot.Telegram;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;

namespace Bounan.Bot.BusinessLogic.Handlers.CallbackQueryHandlers;

public class InfoCallbackQueryHandler(
    ILogger<InfoCallbackQueryHandler> logger,
    InfoMessageHandler infoMessageHandler) : ICallbackQueryHandler
{
    public static bool CanHandle(CallbackQuery item) => item.Data?.StartsWith(InfoCommandDto.Command) ?? false;

    public async Task<AnswerCallbackQueryRequest> HandleAsync(
        CallbackQuery updateItem,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(updateItem);
        ArgumentNullException.ThrowIfNull(updateItem.Data);
        ArgumentNullException.ThrowIfNull(updateItem.Message);
        ArgumentNullException.ThrowIfNull(updateItem.Message.Chat);

        logger.LogInformation("Handling info callback query {Data}", updateItem.Data);

        await infoMessageHandler.HandleAsync(
            new Message
            {
                Chat = updateItem.Message.Chat,
                Text = updateItem.Data,
            },
            cancellationToken);

        return new AnswerCallbackQueryRequest(updateItem.Id);
    }
}