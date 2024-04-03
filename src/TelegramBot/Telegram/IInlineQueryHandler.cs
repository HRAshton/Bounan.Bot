using Telegram.Bot.Requests;
using Telegram.Bot.Types;

namespace Bounan.Bot.TelegramBot.Telegram;

public interface IInlineQueryHandler : IHandler<InlineQuery, Task<AnswerInlineQueryRequest>>;