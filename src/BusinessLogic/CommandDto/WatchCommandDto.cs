using Bounan.Common.Models;

namespace Bounan.Bot.BusinessLogic.CommandDto;

public record WatchCommandDto : ICommandDto, IVideoKey
{
    public static string Command => ":см";

    public required int MyAnimeListId { get; init; }

    public required string Dub { get; init; }

    public required int Episode { get; init; }
}