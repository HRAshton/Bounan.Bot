import { fromJson } from './models';
import { SNSEvent } from 'aws-lambda';
import { process } from './processor';
import { retry } from '../../shared/helpers/retry';
import { setToken } from '../../api-clients/loan-api/src/animan-loan-api-client';
import { config } from '../../config/config';
import { client_setClientToken } from 'telegram-bot-api-lightweight-client';

setToken(config.loanApi.token);
client_setClientToken(config.telegram.token);

const processMessage = async (message: string): Promise<void> => {
    console.log('Processing message: ', message);

    const videoDownloadedNotification = fromJson(message);
    await process(videoDownloadedNotification);

    console.log('Message processed');
};

export const handler = async (event: SNSEvent): Promise<void> => {
    console.log('Processing event: ', JSON.stringify(event));
    for (const record of event.Records) {
        console.log('Processing record: ', record?.Sns?.MessageId);
        await retry(async () => await processMessage(record.Sns.Message), 3, () => true);
    }

    console.info('done');
};
