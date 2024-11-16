import { Stack, StackProps, Duration, CfnOutput } from 'aws-cdk-lib';
import { Construct } from 'constructs';
import * as lambda from 'aws-cdk-lib/aws-lambda';
import * as sns from 'aws-cdk-lib/aws-sns';
import * as subs from 'aws-cdk-lib/aws-sns-subscriptions';
import * as logs from 'aws-cdk-lib/aws-logs';
import * as cw from 'aws-cdk-lib/aws-cloudwatch';
import * as cloudwatchActions from 'aws-cdk-lib/aws-cloudwatch-actions';
import * as apigateway from 'aws-cdk-lib/aws-apigateway';
import { LlrtFunction } from 'cdk-lambda-llrt';

import { config } from './config';

export class AniManCdkStack extends Stack {
    constructor(scope: Construct, id: string, props?: StackProps) {
        super(scope, id, props);

        const logGroup = this.createLogGroup();
        this.setErrorAlarm(logGroup);

        // const tables = this.createTables();
        const functions = this.createLambdas(logGroup);

        const videoDownloadedTopic = sns.Topic.fromTopicArn(
            this, 'VideoDownloadedSnsTopic', config.videoDownloadedTopicArn);
        videoDownloadedTopic.addSubscription(
            new subs.LambdaSubscription(functions.get(LambdaHandler.OnVideoDownloaded)!));

        const getAnimeLambda = lambda.Function.fromFunctionName(
            this, 'GetAnimeLambda', config.getAnimeFunctionName);
        getAnimeLambda.grantInvoke(functions.get(LambdaHandler.OnWebhook)!);

        const apiGateway = new apigateway.RestApi(this, 'WebhookApi', {});
        apiGateway.root.addMethod('POST', new apigateway.LambdaIntegration(functions.get(LambdaHandler.OnWebhook)!));

        this.out('Config', JSON.stringify(config));
        // this.out('Tables', Array.from(tables.values()).map(table => table.tableName));
        functions.forEach((func, key) => this.out(`${key}-LambdaName`, func.functionName));
        this.out(
            'SetWebhookUrl',
            `https://api.telegram.org/bot${config.telegramBotToken}/setWebhook?url=${apiGateway.url}`);
    }

    // private createTables(): Map<Table, dynamodb.Table> {
    //     const usersTable = new dynamodb.Table(this, 'Users', {
    //         partitionKey: { name: 'ChatId', type: dynamodb.AttributeType.NUMBER },
    //         removalPolicy: RemovalPolicy.RETAIN,
    //     });
    //
    //     const subscriptionsTable = new dynamodb.Table(this, 'Subscriptions', {
    //         partitionKey: { name: 'ChatId', type: dynamodb.AttributeType.NUMBER },
    //         sortKey: { name: 'AnimeKey', type: dynamodb.AttributeType.STRING },
    //         removalPolicy: RemovalPolicy.RETAIN,
    //     });
    //
    //     return new Map([
    //         [Table.Users, usersTable],
    //         [Table.Subscriptions, subscriptionsTable],
    //     ]);
    // }

    private createLogGroup(): logs.LogGroup {
        return new logs.LogGroup(this, 'LogGroup', {
            retention: logs.RetentionDays.ONE_WEEK,
        });
    }

    private setErrorAlarm(logGroup: logs.LogGroup): void {
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

    private createLambdas(logGroup: logs.LogGroup): Map<LambdaHandler, lambda.Function> {
        const functions = new Map<LambdaHandler, lambda.Function>();

        Object.entries(LambdaHandler).forEach(([lambdaName, handlerName]) => {
            const func = new LlrtFunction(this, lambdaName, {
                entry: `src/handlers/${handlerName}/handler.ts`,
                handler: 'handler',
                logGroup: logGroup,
                environment: {
                    AWS_PROFILE: 'hra',
                    ANIMAN_GET_ANIME_FUNCTION_NAME: config.getAnimeFunctionName,
                    LOAN_API_TOKEN: config.loanApiToken,
                    LOAN_API_MAX_CONCURRENT_REQUESTS: '6',
                    TELEGRAM_TOKEN: config.telegramBotToken,
                    TELEGRAM_VIDEO_CHAT_ID: config.telegramBotVideoChatId.toString(),
                    TELEGRAM_PUBLISHER_GROUP_NAME: config.telegramBotPublisherGroupName,
                    TELEGRAM_BUTTONS_COLUMNS: '7',
                    TELEGRAM_BUTTONS_ROWS: '3',
                    RETRY_MAX_ATTEMPTS: '1',
                    RETRY_DELAY_MS: '1000',
                    // DATABASE_USERS_TABLE_NAME: 'users',
                    // DATABASE_SUBSCRIPTIONS_TABLE_NAME: 'subscriptions',
                },
                timeout: Duration.seconds(30),
            });

            functions.set(handlerName, func);
        });

        return functions;
    }

    private out(key: string, value: object | string): void {
        const output = typeof value === 'string' ? value : JSON.stringify(value);
        new CfnOutput(this, key, { value: output });
    }
}

// enum Table {
//     Users = 'Users',
//     Subscriptions = 'Subscriptions',
// }

enum LambdaHandler {
    OnWebhook = 'on-webhook',
    OnVideoDownloaded = 'on-video-downloaded',
}
