using Bounan.Bot.BusinessLogic.Handlers.CallbackQueryHandlers;
using Bounan.Bot.BusinessLogic.Handlers.InlineQueryHandlers;
using Bounan.Bot.BusinessLogic.Handlers.MessageHandlers;
using Bounan.Bot.BusinessLogic.Interfaces;
using Bounan.Bot.TelegramBot.Telegram;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Bounan.Bot.BusinessLogic.Services;

internal class BotService : IBotService
{
    public BotService(ILogger<BotService> logger, ITelegramWrapper telegramWrapper)
    {
        Logger = logger;
        TelegramWrapper = telegramWrapper;

        Initialize();
    }

    private ILogger<BotService> Logger { get; }

    private ITelegramWrapper TelegramWrapper { get; }

    public Task HandleUpdateAsync(Update? update, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(update, nameof(update));

        Logger.LogInformation("Handling update: {Type}", update.Type);
        return TelegramWrapper.HandleAsync(update, cancellationToken);
    }

    private void Initialize()
    {
        TelegramWrapper.OnMessage<StartMessageHandler>();
        TelegramWrapper.OnMessage<KnownInlineAnswerMessageHandler>();
        TelegramWrapper.OnMessage<InfoMessageHandler>();
        TelegramWrapper.OnMessage<WatchMessageHandler>();
        TelegramWrapper.OnMessageDefault<SearchMessageHandler>();

        TelegramWrapper.OnCallbackQuery<InfoCallbackQueryHandler>();
        TelegramWrapper.OnCallbackQuery<WatchCallbackQueryHandler>();

        TelegramWrapper.OnInlineQuery<DubsInlineQueryHandler>();
        TelegramWrapper.OnInlineQuery<RelatedInlineQueryHandler>();
        TelegramWrapper.OnInlineQueryDefault<SearchInlineQueryHandler>();
    }
}