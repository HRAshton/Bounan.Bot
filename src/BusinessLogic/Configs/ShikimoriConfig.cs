namespace Bounan.Bot.BusinessLogic.Configs;

public class ShikimoriConfig
{
    public const string SectionName = "Shikimori";

    public required Uri BaseUrl { get; init; } = new("https://shikimori.one");
}