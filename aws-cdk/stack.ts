import { Construct } from 'constructs';
import { LlrtFunction } from 'cdk-lambda-llrt';

import * as cdk from 'aws-cdk-lib';
import * as lambda from 'aws-cdk-lib/aws-lambda';
import * as sns from 'aws-cdk-lib/aws-sns';
import * as subs from 'aws-cdk-lib/aws-sns-subscriptions';
import * as logs from 'aws-cdk-lib/aws-logs';
import * as cw from 'aws-cdk-lib/aws-cloudwatch';
import * as dynamodb from 'aws-cdk-lib/aws-dynamodb';
import * as cloudwatchActions from 'aws-cdk-lib/aws-cloudwatch-actions';
import * as apigateway from 'aws-cdk-lib/aws-apigateway';

import { Config, getConfig } from './config';

export class Stack extends cdk.Stack {
    constructor(scope: Construct, id: string, props?: cdk.StackProps) {
        super(scope, id, props);

        const config = getConfig(this, 'bounan:', '/bounan/bot/deploy-config/');

        const logGroup = this.createLogGroup();

        const tables = this.createTables();
        const functions = this.createLambdas(logGroup, tables, config);
        this.setErrorAlarm(logGroup, config);

        const videoDownloadedTopic = sns.Topic.fromTopicArn(
            this, 'VideoDownloadedSnsTopic', config.videoDownloadedTopicArn);
        videoDownloadedTopic.addSubscription(
            new subs.LambdaSubscription(functions[LambdaHandler.OnVideoDownloaded]));

        const getAnimeLambda = lambda.Function.fromFunctionName(
            this, 'GetAnimeLambda', config.getAnimeFunctionName);
        getAnimeLambda.grantInvoke(functions[LambdaHandler.OnWebhook]);

        const apiGateway = new apigateway.RestApi(this, 'WebhookApi', {});
        apiGateway.root.addMethod('POST', new apigateway.LambdaIntegration(functions[LambdaHandler.OnWebhook]));

        this.out('Config', JSON.stringify(config));
        this.out(
            'SetWebhookUrl',
            `https://api.telegram.org/bot${config.telegramBotToken}/setWebhook?url=${apiGateway.url}`);
    }

    private createTables(): Record<Table, dynamodb.Table> {
        const usersTable = new dynamodb.Table(this, Table.Users, {
            partitionKey: { name: 'userId', type: dynamodb.AttributeType.NUMBER },
            removalPolicy: cdk.RemovalPolicy.RETAIN,
        });

        const subscriptionsTable = new dynamodb.Table(this, Table.Subscriptions, {
            partitionKey: { name: 'animeKey', type: dynamodb.AttributeType.STRING },
            sortKey: { name: 'userId', type: dynamodb.AttributeType.NUMBER },
            removalPolicy: cdk.RemovalPolicy.RETAIN,
        });

        return {
            [Table.Users]: usersTable,
            [Table.Subscriptions]: subscriptionsTable,
        }
    }

    private createLogGroup(): logs.LogGroup {
        return new logs.LogGroup(this, 'LogGroup', { retention: logs.RetentionDays.ONE_WEEK });
    }

    private setErrorAlarm(logGroup: logs.LogGroup, config: Config): void {
        const topic = new sns.Topic(this, 'LogGroupAlarmSnsTopic');
        topic.addSubscription(new subs.EmailSubscription(config.alertEmail));

        const metricFilter = logGroup.addMetricFilter('ErrorMetricFilter', {
            filterPattern: logs.FilterPattern.anyTerm('ERROR', 'Error', 'error', 'fail'),
            metricNamespace: this.stackName,
            metricName: 'ErrorCount',
            metricValue: '1',
        });

        const alarm = new cw.Alarm(this, 'LogGroupErrorAlarm', {
            metric: metricFilter.metric(),
            threshold: 1,
            evaluationPeriods: 1,
            treatMissingData: cw.TreatMissingData.NOT_BREACHING,
        });

        alarm.addAlarmAction(new cloudwatchActions.SnsAction(topic));
    }

    private createLambdas(
        logGroup: logs.LogGroup,
        tables: Record<Table, dynamodb.Table>,
        config: Config,
    ): Record<LambdaHandler, lambda.Function> {
        // @ts-expect-error - we know that the keys are the same
        const functions: Record<LambdaHandler, lambda.Function> = {};

        Object.entries(LambdaHandler).forEach(([lambdaName, handlerName]) => {
            const func = new LlrtFunction(this, lambdaName, {
                entry: `src/handlers/${handlerName}/handler.ts`,
                handler: 'handler',
                logGroup: logGroup,
                environment: {
                    AWS_PROFILE: 'hra',
                    ANIMAN_GET_ANIME_FUNCTION_NAME: config.getAnimeFunctionName,
                    DATABASE_USERS_TABLE_NAME: tables[Table.Users].tableName,
                    DATABASE_SUBSCRIPTIONS_TABLE_NAME: tables[Table.Subscriptions].tableName,
                    LOAN_API_TOKEN: config.loanApiToken,
                    LOAN_API_MAX_CONCURRENT_REQUESTS: '6',
                    TELEGRAM_TOKEN: config.telegramBotToken,
                    TELEGRAM_VIDEO_CHAT_ID: config.telegramBotVideoChatId.toString(),
                    TELEGRAM_PUBLISHER_GROUP_NAME: config.telegramBotPublisherGroupName,
                    TELEGRAM_BUTTONS_COLUMNS: '7',
                    TELEGRAM_BUTTONS_ROWS: '3',
                    RETRY_MAX_ATTEMPTS: '1',
                    RETRY_DELAY_MS: '1000',
                    STUDIO_LOGOS_URL: config.studioLogosUrl,
                },
                timeout: cdk.Duration.seconds(30),
            });

            functions[handlerName] = func;
        });

        return functions;
    }

    private out(key: string, value: object | string): void {
        const output = typeof value === 'string' ? value : JSON.stringify(value);
        new cdk.CfnOutput(this, key, { value: output });
    }
}

enum Table {
    Users = 'users',
    Subscriptions = 'subscriptions',
}

enum LambdaHandler {
    OnWebhook = 'on-webhook',
    OnVideoDownloaded = 'on-video-downloaded',
}
