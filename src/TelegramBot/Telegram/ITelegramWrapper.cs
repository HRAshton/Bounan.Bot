using Telegram.Bot.Types;

namespace Bounan.Bot.TelegramBot.Telegram;

public interface ITelegramWrapper
{
    void OnMessageDefault<TMessageHandler>()
        where TMessageHandler : IMessageHandler;

    void OnCallbackQueryDefault<TCallbackQueryHandler>()
        where TCallbackQueryHandler : ICallbackQueryHandler;

    void OnInlineQueryDefault<TInlineQueryHandler>()
        where TInlineQueryHandler : IInlineQueryHandler;

    void OnMessage<TMessageHandler>()
        where TMessageHandler : IMessageHandler;

    void OnCallbackQuery<TCallbackQueryHandler>()
        where TCallbackQueryHandler : ICallbackQueryHandler;

    void OnInlineQuery<TInlineQueryHandler>()
        where TInlineQueryHandler : IInlineQueryHandler;

    Task HandleAsync(Update? update, CancellationToken cancellationToken);
}