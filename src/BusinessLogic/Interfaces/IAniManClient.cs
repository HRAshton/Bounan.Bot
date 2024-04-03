using Bounan.Common.Models;

namespace Bounan.Bot.BusinessLogic.Interfaces;

public interface IAniManClient
{
    Task<IBotResponse?> GetAnimeAsync(IBotRequest request, CancellationToken cancellationToken);
}