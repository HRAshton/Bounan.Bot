using Bounan.Bot.TelegramBot.Configuration;
using Bounan.Bot.TelegramBot.Telegram;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace Bounan.Bot.TelegramBot;

public static class Bootstrap
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ITelegramWrapper, TelegramWrapper>();

        services.AddSingleton<ITelegramBotClient, TelegramBotClient>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<TelegramBotConfig>>();
            return new TelegramBotClient(config.Value.BotToken);
        });
    }

    public static void AddConfiguration(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TelegramBotConfig>(configuration.GetSection(TelegramBotConfig.SectionName));
    }
}