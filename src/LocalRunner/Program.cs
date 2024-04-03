using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Amazon.SQS;
using Bounan.Bot.Endpoint;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;

Console.WriteLine("LocalRunner");

var lambdaHandlers = new LambdaHandlers();
var poolingBot = new TelegramBotClient(Environment.GetEnvironmentVariable("TelegramBot__BotToken")!);
var lambdaContext = new TestLambdaContext();

poolingBot.StartReceiving(
    UpdateHandler,
    pollingErrorHandler: (_, exception, _) => Console.WriteLine(exception),
    receiverOptions: null,
    cancellationToken: default);

var sqsClient = new AmazonSQSClient();
var queueUrl = Environment.GetEnvironmentVariable("LocalRunner__NotificationQueueUrl")!;
while (true)
{
    var response = await sqsClient.ReceiveMessageAsync(queueUrl);
    if (response.Messages.Count == 0)
    {
        await Task.Delay(1000);
        continue;
    }

    foreach (var message in response.Messages)
    {
        var payload = message.Body;
        await lambdaHandlers.NotificationFromAniManInternal(payload, lambdaContext);
        await sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle!);
    }
}

async void UpdateHandler(ITelegramBotClient telegramBotClient, Update update, CancellationToken cancellationToken)
{
    try
    {
        var request = new APIGatewayProxyRequest { Body = JsonConvert.SerializeObject(update) };

        await lambdaHandlers.TelegramEvent(request, lambdaContext);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}