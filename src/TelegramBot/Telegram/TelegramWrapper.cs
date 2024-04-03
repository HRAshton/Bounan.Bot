using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bounan.Bot.TelegramBot.Telegram;

internal class TelegramWrapper(
    ILogger<TelegramWrapper> logger,
    IServiceProvider serviceProvider,
    ITelegramBotClient botClient) : ITelegramWrapper
{
    private readonly LinkedList<(Func<Message, bool> CanHandle, Type HandlerType)> _messageHandlers = [];

    private readonly LinkedList<(Func<CallbackQuery, bool> CanHandle, Type HandlerType)> _callbackQueryHandlers = [];

    private readonly LinkedList<(Func<InlineQuery, bool> CanHandle, Type HandlerType)> _inlineQueryHandlers = [];

    private Type? _defaultMessageHandler;

    private Type? _defaultCallbackQueryHandlerType;

    private Type? _defaultInlineQueryHandlerType;

    private ILogger<TelegramWrapper> Logger { get; } = logger;

    public void OnMessageDefault<TMessageHandler>()
        where TMessageHandler : IMessageHandler
    {
        SetDefault(typeof(TMessageHandler), ref _defaultMessageHandler);
    }

    public void OnCallbackQueryDefault<TCallbackQueryHandler>()
        where TCallbackQueryHandler : ICallbackQueryHandler
    {
        SetDefault(typeof(TCallbackQueryHandler), ref _defaultCallbackQueryHandlerType);
    }

    public void OnInlineQueryDefault<TInlineQueryHandler>()
        where TInlineQueryHandler : IInlineQueryHandler
    {
        SetDefault(typeof(TInlineQueryHandler), ref _defaultInlineQueryHandlerType);
    }

    public void OnMessage<TMessageHandler>()
        where TMessageHandler : IMessageHandler
    {
        _messageHandlers.AddLast((TMessageHandler.CanHandle, typeof(TMessageHandler)));
    }

    public void OnCallbackQuery<TCallbackQueryHandler>()
        where TCallbackQueryHandler : ICallbackQueryHandler
    {
        _callbackQueryHandlers.AddLast((TCallbackQueryHandler.CanHandle, typeof(TCallbackQueryHandler)));
    }

    public void OnInlineQuery<TInlineQueryHandler>()
        where TInlineQueryHandler : IInlineQueryHandler
    {
        _inlineQueryHandlers.AddLast((TInlineQueryHandler.CanHandle, typeof(TInlineQueryHandler)));
    }

    public async Task HandleAsync(
        Update? update,
        CancellationToken cancellationToken)
    {
        Logger.LogDebug("Handling update: {@Update}", update);

        switch (update?.Type)
        {
            case UpdateType.Message when update.Message?.Text is not null:
            {
                Logger.LogDebug("Handling as Message");

                if (!TryGetHandlerType(update.Message, _messageHandlers, _defaultMessageHandler, out var handlerType))
                {
                    break;
                }

                ArgumentNullException.ThrowIfNull(handlerType);
                var handler = serviceProvider.GetRequiredService(handlerType) as IHandler<Message, Task>;

                ArgumentNullException.ThrowIfNull(handler);
                await handler.HandleAsync(update.Message, cancellationToken);

                break;
            }

            case UpdateType.CallbackQuery when update.CallbackQuery?.Data is not null:
            {
                Logger.LogDebug("Handling as CallbackQuery");

                if (!TryGetHandlerType(
                        update.CallbackQuery,
                        _callbackQueryHandlers,
                        _defaultCallbackQueryHandlerType,
                        out var handlerType))
                {
                    break;
                }

                ArgumentNullException.ThrowIfNull(handlerType);
                var handler = serviceProvider.GetRequiredService(handlerType)
                    as IHandler<CallbackQuery, Task<AnswerCallbackQueryRequest>>;

                ArgumentNullException.ThrowIfNull(handler);
                var results = await handler.HandleAsync(update.CallbackQuery, cancellationToken);
                await botClient.MakeRequestAsync(results, cancellationToken);

                break;
            }

            case UpdateType.InlineQuery when update.InlineQuery?.Query is not null:
            {
                Logger.LogDebug("Handling as InlineQuery");

                if (!TryGetHandlerType(
                        update.InlineQuery,
                        _inlineQueryHandlers,
                        _defaultInlineQueryHandlerType,
                        out var handlerType))
                {
                    break;
                }

                ArgumentNullException.ThrowIfNull(handlerType);
                var handler = serviceProvider.GetRequiredService(handlerType)
                    as IHandler<InlineQuery, Task<AnswerInlineQueryRequest>>;

                ArgumentNullException.ThrowIfNull(handler);
                var results = await handler.HandleAsync(update.InlineQuery, cancellationToken);
                await botClient.MakeRequestAsync(results, cancellationToken);

                break;
            }
        }
    }

    private static void SetDefault(Type handler, ref Type? defaultHandlerType)
    {
        if (defaultHandlerType is not null)
        {
            throw new InvalidOperationException("Default inline query handler already set.");
        }

        defaultHandlerType = handler;
    }

    private bool TryGetHandlerType<TUpdate>(
        TUpdate arg,
        IEnumerable<(Func<TUpdate, bool> CanHandle, Type HandlerType)> handlers,
        Type? defaultHandler,
        out Type? handlerType)
    {
        handlerType = handlers
                          .SingleOrDefault(pair => pair.CanHandle(arg))
                          .HandlerType
                      ?? defaultHandler;
        if (handlerType is null)
        {
            Logger.LogDebug("No handler found");
            return false;
        }

        Logger.LogDebug("Handler resolved: {HandlerType}", handlerType);
        return true;
    }
}