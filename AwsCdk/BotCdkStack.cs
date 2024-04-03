using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.CloudWatch;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SNS.Subscriptions;
using Amazon.CDK.AWS.SQS;
using Constructs;
using Microsoft.Extensions.Configuration;
using AssetOptions = Amazon.CDK.AWS.S3.Assets.AssetOptions;
using Targets = Amazon.CDK.AWS.Events.Targets;

namespace Bounan.Downloader.AwsCdk;

[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "This is a stack.")]
public class BotCdkStack : Stack
{
    internal BotCdkStack(Construct scope, string id, IStackProps? props = null)
        : base(scope, id, props)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build()
            .Get<BounanCdkStackConfig>();
        ArgumentNullException.ThrowIfNull(config, nameof(config));
        config.Validate();

        var logGroup = CreateLogGroup();
        SetErrorAlarm(config, logGroup);

        var aniManLambda = Function.FromFunctionName(this, "AniManLambda", config.GetAnimeFunctionName);
        var (webhookHandler, notificationHandler) = CreateLambda(config, logGroup);
        aniManLambda.GrantInvoke(webhookHandler);

        CreateWarmer(config, webhookHandler);

        var apiGateway = new RestApi(this, "WebhookApi", new RestApiProps());
        apiGateway.Root.AddMethod("POST", new LambdaIntegration(webhookHandler));

        var notificationQueue = Queue.FromQueueArn(this, "NotificationQueue", config.NotificationQueueArn);
        notificationHandler.AddEventSource(new SqsEventSource(notificationQueue));

        var registrationUrl = $"https://api.telegram.org/bot{config.TelegramBotToken}/setWebhook?url={apiGateway.Url}";

        Out("Bounan.Downloader.WebhookRegisterUrl", registrationUrl);
        Out("Bounan.Downloader.LogGroupName", logGroup.LogGroupName);
        Out("Bounan.Downloader.WebhookHandlerUrl", apiGateway.Url);
    }

    private LogGroup CreateLogGroup()
    {
        return new LogGroup(this, "LogGroup", new LogGroupProps
        {
            Retention = RetentionDays.ONE_WEEK,
        });
    }

    private void SetErrorAlarm(BounanCdkStackConfig bounanCdkStackConfig, ILogGroup logGroup)
    {
        var topic = new Topic(this, "LogGroupAlarmSnsTopic", new TopicProps());

        topic.AddSubscription(new EmailSubscription(bounanCdkStackConfig.AlertEmail));

        var errorPattern = new MetricFilter(this, "LogGroupErrorPattern", new MetricFilterProps
        {
            LogGroup = logGroup,
            FilterPattern = FilterPattern.AnyTerm("ERROR", "Error", "error"),
            MetricNamespace = "MetricNamespace",
            MetricName = "ErrorCount",
        });

        _ = new Alarm(this, "LogGroupErrorAlarm", new AlarmProps
        {
            Metric = errorPattern.Metric(),
            Threshold = 1,
            EvaluationPeriods = 1,
            TreatMissingData = TreatMissingData.NOT_BREACHING,
        });
    }

    private (Function WebhookHandler, Function NotificationHandler) CreateLambda(
        BounanCdkStackConfig bounanCdkStackConfig,
        ILogGroup logGroup)
    {
        var asset = Code.FromAsset("src", new AssetOptions
        {
            Bundling = new BundlingOptions
            {
                Image = Runtime.DOTNET_8.BundlingImage,
                User = "root",
                OutputType = BundlingOutput.ARCHIVED,
                Command =
                [
                    "/bin/sh",
                    "-c",
                    " dotnet tool install -g Amazon.Lambda.Tools" +
                    " && dotnet build Endpoint" +
                    " && dotnet lambda package --output-package /asset-output/function.zip --project-location Endpoint"
                ],
            },
        });

        var webhookHandler = new Function(this, "WebhookEndpoint", new FunctionProps
        {
            Runtime = Runtime.DOTNET_8,
            Code = asset,
            Handler = "Bounan.Bot.Endpoint::Bounan.Bot.Endpoint.LambdaHandlers::TelegramEvent",
            Timeout = Duration.Seconds(30),
            LogGroup = logGroup,
            Environment = new Dictionary<string, string>
            {
                { "LoanApi__Token", bounanCdkStackConfig.LoanApiToken },
                { "AniMan__GetAnimeFunctionName", bounanCdkStackConfig.GetAnimeFunctionName },
                { "TelegramBot__BotToken", bounanCdkStackConfig.TelegramBotToken },
            },
        });

        var notificationHandler = new Function(this, "NotificationEndpoint", new FunctionProps
        {
            Runtime = Runtime.DOTNET_8,
            Code = asset,
            Handler = "Bounan.Bot.Endpoint::Bounan.Bot.Endpoint.LambdaHandlers::NotificationFromAniMan",
            Timeout = Duration.Seconds(30),
            LogGroup = logGroup,
            Environment = new Dictionary<string, string>
            {
                { "LoanApi__Token", bounanCdkStackConfig.LoanApiToken },
                { "TelegramBot__BotToken", bounanCdkStackConfig.TelegramBotToken },
            },
        });

        return (webhookHandler, notificationHandler);
    }

    private void CreateWarmer(BounanCdkStackConfig bounanCdkStackConfig, IFunction webhookHandler)
    {
        var rule = new Rule(this, "WarmerRule", new RuleProps
        {
            Schedule = Schedule.Rate(Duration.Minutes(bounanCdkStackConfig.WarmupTimeoutMinutes)),
        });

        rule.AddTarget(new Targets.LambdaFunction(webhookHandler));
    }

    private void Out(string key, string value)
    {
        _ = new CfnOutput(this, key, new CfnOutputProps { Value = value });
    }
}