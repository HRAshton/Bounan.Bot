namespace Bounan.Bot.TelegramBot.Configuration;

internal record TelegramBotConfig
{
    public const string SectionName = "TelegramBot";

    public required string BotToken { get; init; }
}