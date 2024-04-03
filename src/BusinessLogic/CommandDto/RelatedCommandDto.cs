namespace Bounan.Bot.BusinessLogic.CommandDto;

public record RelatedCommandDto : ICommandDto
{
    public static string Command => ":св";

    public required int MyAnimeListId { get; init; }
}