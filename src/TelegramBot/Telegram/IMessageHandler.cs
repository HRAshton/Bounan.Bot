using Telegram.Bot.Types;

namespace Bounan.Bot.TelegramBot.Telegram;

public interface IMessageHandler : IHandler<Message, Task>;