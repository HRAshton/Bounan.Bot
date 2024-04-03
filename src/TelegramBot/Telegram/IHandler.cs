namespace Bounan.Bot.TelegramBot.Telegram;

public interface IHandler<in TUpdateItem, out TResult>
    where TResult : Task
{
    static abstract bool CanHandle(TUpdateItem item);

    TResult HandleAsync(TUpdateItem updateItem, CancellationToken cancellationToken);
}