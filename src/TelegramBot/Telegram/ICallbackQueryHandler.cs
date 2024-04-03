using Telegram.Bot.Requests;
using Telegram.Bot.Types;

namespace Bounan.Bot.TelegramBot.Telegram;

public interface ICallbackQueryHandler : IHandler<CallbackQuery, Task<AnswerCallbackQueryRequest>>;