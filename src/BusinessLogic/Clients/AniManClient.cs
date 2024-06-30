using System.Net;
using System.Text;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Bounan.Bot.BusinessLogic.Configs;
using Bounan.Bot.BusinessLogic.Interfaces;
using Bounan.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Bounan.Bot.BusinessLogic.Clients;

public class AniManClient : IAniManClient
{
    public AniManClient(
        ILogger<AniManClient> logger,
        IOptions<AniManConfig> aniManConfig,
        IAmazonLambda lambdaClient)
    {
        LambdaClient = lambdaClient;
        Logger = logger;
        AniManConfig = aniManConfig;
    }

    private ILogger<AniManClient> Logger { get; }

    private IOptions<AniManConfig> AniManConfig { get; }

    private IAmazonLambda LambdaClient { get; }

    public async Task<BotResponse?> GetAnimeAsync(
        BotRequest request,
        CancellationToken cancellationToken)
    {
        var lambdaRequest = new InvokeRequest
        {
            FunctionName = AniManConfig.Value.GetAnimeFunctionName,
            InvocationType = InvocationType.RequestResponse,
            Payload = JsonConvert.SerializeObject(request),
        };

        var response = await LambdaClient.InvokeAsync(lambdaRequest, cancellationToken);
        if (response.HttpStatusCode != HttpStatusCode.OK)
        {
            Logger.LogError("Failed to update video status. Response: {@Response}", response);
            return null;
        }

        Logger.LogInformation("Video status updated. Request: {@Request}", request);
        var responsePayload = Encoding.UTF8.GetString(response.Payload.ToArray());
        Logger.LogDebug("Response payload: {ResponsePayload}", responsePayload);

        var result = JsonConvert.DeserializeObject<BotResponse>(responsePayload);
        return result;
    }
}