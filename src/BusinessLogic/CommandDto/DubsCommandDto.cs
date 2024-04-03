namespace Bounan.Bot.BusinessLogic.CommandDto;

public record DubsCommandDto : ICommandDto
{
    public static string Command => ":озв";

    public required int MyAnimeListId { get; init; }
}