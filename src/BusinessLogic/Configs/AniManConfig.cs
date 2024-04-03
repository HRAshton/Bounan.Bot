namespace Bounan.Bot.BusinessLogic.Configs;

public record AniManConfig
{
    public const string SectionName = "AniMan";

    public required string GetAnimeFunctionName { get; init; }
}