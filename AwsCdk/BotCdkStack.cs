using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.CloudWatch;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SNS.Subscriptions;
using Constructs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using AlarmActions = Amazon.CDK.AWS.CloudWatch.Actions;
using AssetOptions = Amazon.CDK.AWS.S3.Assets.AssetOptions;
using Targets = Amazon.CDK.AWS.Events.Targets;

namespace Bounan.Downloader.AwsCdk;

public class BotCdkStack : Stack
{
    internal BotCdkStack(Construct scope, string id, IStackProps props)
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

        var getAnimeLambda = Function.FromFunctionName(this, "GetAnimeLambda", config.GetAnimeFunctionName);
        var functions = CreateLambda(config, logGroup);
        getAnimeLambda.GrantInvoke(functions[LambdaHandler.TelegramEvent]);

        CreateWarmer(config, functions[LambdaHandler.TelegramEvent]);

        var apiGateway = new RestApi(this, "WebhookApi", new RestApiProps());
        apiGateway.Root.AddMethod("POST", new LambdaIntegration(functions[LambdaHandler.TelegramEvent]));

        var notificationTopic = Topic.FromTopicArn(this, "VideoDownloadedTopic", config.VideoDownloadedTopicArn);
        functions[LambdaHandler.NotificationFromAniMan].AddEventSource(new SnsEventSource(notificationTopic));

        Out("Config", JsonConvert.SerializeObject(config));
        Out("LogGroupName", logGroup.LogGroupName);
        Out("WebhookHandlerUrl", apiGateway.Url);
        Out(
            "WebhookRegisterUrl",
            $"https://api.telegram.org/bot{config.TelegramBotToken}/setWebhook?url={apiGateway.Url}");
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

        var metricFilter = logGroup.AddMetricFilter("ErrorMetricFilter", new MetricFilterOptions
        {
            FilterPattern = FilterPattern.AnyTerm("ERROR", "Error", "error", "fail"),
            MetricNamespace = StackName,
            MetricName = "ErrorCount",
            MetricValue = "1",
        });

        var alarm = new Alarm(this, "LogGroupErrorAlarm", new AlarmProps
        {
            Metric = metricFilter.Metric(),
            Threshold = 1,
            EvaluationPeriods = 1,
            TreatMissingData = TreatMissingData.NOT_BREACHING,
        });
        alarm.AddAlarmAction(new AlarmActions.SnsAction(topic));
    }

    private IDictionary<LambdaHandler, Function> CreateLambda(
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

        var functions = Enum.GetValues<LambdaHandler>()
            .ToDictionary(
                name => name,
                name => new Function(this, name.ToString(), new FunctionProps
                {
                    Runtime = Runtime.DOTNET_8,
                    Code = asset,
                    Handler = $"Bounan.Bot.Endpoint::Bounan.Bot.Endpoint.LambdaHandlers::{name}",
                    Timeout = Duration.Seconds(30),
                    LogGroup = logGroup,
                    Environment = new Dictionary<string, string>
                    {
                        { "Logging__LogLevel__Default", "Information" },
                        { "Logging__LogLevel__System", "Warning" },
                        { "LoanApi__Token", bounanCdkStackConfig.LoanApiToken },
                        { "AniMan__GetAnimeFunctionName", bounanCdkStackConfig.GetAnimeFunctionName },
                        { "TelegramBot__BotToken", bounanCdkStackConfig.TelegramBotToken },
                        { "TelegramBot__VideoChatId", bounanCdkStackConfig.TelegramBotVideoChatId },
                        { "TelegramBot__ForwardingChatId", bounanCdkStackConfig.TelegramBotForwardingChatId },
                        { "TelegramBot__PublisherGroup", bounanCdkStackConfig.TelegramBotPublisherGroup },
                    },
                }));

        return functions;
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

    private enum LambdaHandler
    {
        TelegramEvent,
        NotificationFromAniMan,
    }
}