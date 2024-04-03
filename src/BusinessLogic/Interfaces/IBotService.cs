using Telegram.Bot.Types;

namespace Bounan.Bot.BusinessLogic.Interfaces;

public interface IBotService
{
    Task HandleUpdateAsync(Update? update, CancellationToken cancellationToken);
}