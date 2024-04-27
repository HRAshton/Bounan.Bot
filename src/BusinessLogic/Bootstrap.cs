using Amazon.Lambda;
using Bounan.Bot.BusinessLogic.Clients;
using Bounan.Bot.BusinessLogic.Clients.Shikimori;
using Bounan.Bot.BusinessLogic.Configs;
using Bounan.Bot.BusinessLogic.Handlers.CallbackQueryHandlers;
using Bounan.Bot.BusinessLogic.Handlers.InlineQueryHandlers;
using Bounan.Bot.BusinessLogic.Handlers.MessageHandlers;
using Bounan.Bot.BusinessLogic.Interfaces;
using Bounan.Bot.BusinessLogic.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;

namespace Bounan.Bot.BusinessLogic;

public static class Bootstrap
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IBotService, BotService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<IAmazonLambda, AmazonLambdaClient>();
        services.AddSingleton<IAniManClient, AniManClient>();
        services.AddSingleton<IFileIdFinder, FileIdFinder>();

        services.AddRefitClient<IShikimoriApi>()
            .ConfigureHttpClient((serviceProvider, client) =>
            {
                client.BaseAddress = serviceProvider.GetRequiredService<IOptions<ShikimoriConfig>>().Value.BaseUrl;
            });

        services.AddSingleton<StartMessageHandler>();
        services.AddSingleton<KnownInlineAnswerMessageHandler>();
        services.AddSingleton<InfoMessageHandler>();
        services.AddSingleton<WatchMessageHandler>();
        services.AddSingleton<SearchMessageHandler>();

        services.AddSingleton<InfoCallbackQueryHandler>();
        services.AddSingleton<WatchCallbackQueryHandler>();

        services.AddSingleton<DubsInlineQueryHandler>();
        services.AddSingleton<RelatedInlineQueryHandler>();
        services.AddSingleton<SearchInlineQueryHandler>();
    }

    public static void AddConfiguration(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TelegramBotConfig>(configuration.GetSection(TelegramBotConfig.SectionName));
        services.Configure<ShikimoriConfig>(configuration.GetSection(ShikimoriConfig.SectionName));
        services.Configure<RetryPolicyConfig>(configuration.GetSection(RetryPolicyConfig.SectionName));
        services.Configure<AniManConfig>(configuration.GetSection(AniManConfig.SectionName));
    }
}