using JetBrains.Annotations;

namespace Bounan.Bot.BusinessLogic.Clients.Shikimori.Models;

public record AnimeImage
{
    public required Uri Preview { get; [UsedImplicitly] init; }
}