namespace Bounan.Bot.BusinessLogic.Configs;

public class TelegramBotConfig
{
    public const string SectionName = "TelegramBot";

    public ButtonsPagination ButtonsPagination { get; init; } = new();
}