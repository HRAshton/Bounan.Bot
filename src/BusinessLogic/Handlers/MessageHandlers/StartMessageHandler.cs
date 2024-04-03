using Bounan.Bot.TelegramBot.Telegram;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bounan.Bot.BusinessLogic.Handlers.MessageHandlers;

public class StartMessageHandler(
    ILogger<StartMessageHandler> logger,
    ITelegramBotClient botClient) : IMessageHandler
{
    public static bool CanHandle(Message message) => message.Text?.StartsWith("/start") ?? false;

    public Task HandleAsync(Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.Chat);
        ArgumentNullException.ThrowIfNull(message.Chat.Id);
        ArgumentNullException.ThrowIfNull(message.Text);

        logger.LogInformation("Handling start message {Text}", message.Text);

        return botClient.SendTextMessageAsync(
            message.Chat.Id,
            "Напиши название аниме или нажми на кнопку \"Искать\", и я найду его для тебя",
            replyMarkup: new InlineKeyboardMarkup([
                new InlineKeyboardButton("Искать") { SwitchInlineQueryCurrentChat = string.Empty }
            ]),
            cancellationToken: cancellationToken);
    }
}