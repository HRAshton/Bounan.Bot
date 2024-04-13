using Amazon.Lambda.Annotations;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Bounan.Bot.BusinessLogic.Interfaces;
using Bounan.Bot.BusinessLogic.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot.Types;

[assembly: LambdaGlobalProperties(GenerateMain = true)]
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Bounan.Bot.Endpoint;

public class LambdaHandlers
{
    public LambdaHandlers()
    {
        var services = new ServiceCollection();
        Bootstrap.ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();

        Logger = ServiceProvider.GetRequiredService<ILogger<LambdaHandlers>>();
    }

    private ILogger<LambdaHandlers> Logger { get; }

    private ServiceProvider ServiceProvider { get; }

    [LambdaFunction]
    public async Task<APIGatewayProxyResponse> TelegramEvent(APIGatewayProxyRequest? request, ILambdaContext context)
    {
        try
        {
            if (request?.Body is null)
            {
                context.Logger.LogLine("No body in request. Warmup? Returning 202.");
                return new APIGatewayProxyResponse { StatusCode = 202 };
            }

            var payload = request.Body;
            var update = JsonConvert.DeserializeObject<Update>(payload);

            var service = ServiceProvider.GetRequiredService<IBotService>();
            await service.HandleUpdateAsync(update, CancellationToken.None);

            return new APIGatewayProxyResponse { StatusCode = 200 };
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error handling request: {@Request}", request);

            // Returning 202 to not retry the request
            return new APIGatewayProxyResponse { StatusCode = 202 };
        }
    }

    [LambdaFunction]
    public async Task NotificationFromAniMan(SQSEvent sqsEvent, ILambdaContext context)
    {
        foreach (var record in sqsEvent.Records)
        {
            var payload = record.Body;
            await NotificationFromAniManInternal(payload, context);
        }
    }

    public async Task NotificationFromAniManInternal(string payload, ILambdaContext context)
    {
        var update = JsonConvert.DeserializeObject<BotNotification>(payload);
        ArgumentNullException.ThrowIfNull(update);

        var service = ServiceProvider.GetRequiredService<INotificationService>();
        await service.HandleAsync(update);
    }
}