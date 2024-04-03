namespace Bounan.Bot.BusinessLogic.CommandDto;

public record InfoCommandDto : ICommandDto
{
    public static string Command => ":инф";

    public required int MyAnimeListId { get; init; }
}