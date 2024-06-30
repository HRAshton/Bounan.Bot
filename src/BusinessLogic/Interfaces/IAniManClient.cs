using Bounan.Common;

namespace Bounan.Bot.BusinessLogic.Interfaces;

public interface IAniManClient
{
    Task<BotResponse?> GetAnimeAsync(BotRequest request, CancellationToken cancellationToken);
}