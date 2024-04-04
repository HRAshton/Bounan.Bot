using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bounan.Bot.Endpoint;

public static class Bootstrap
{
    public static void ConfigureServices(IServiceCollection services)
    {
        LoanApi.Registrar.RegisterBotServices(services);
        BusinessLogic.Bootstrap.ConfigureServices(services);
        TelegramBot.Bootstrap.ConfigureServices(services);

        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        AddLogging(services, configuration);

        LoanApi.Registrar.RegisterConfiguration(services, configuration);
        BusinessLogic.Bootstrap.AddConfiguration(services, configuration);
        TelegramBot.Bootstrap.AddConfiguration(services, configuration);
    }

    private static void AddLogging(IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(builder =>
        {
            builder.AddConfiguration(configuration.GetSection("Logging"));
            builder.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
            });
        });
    }
}