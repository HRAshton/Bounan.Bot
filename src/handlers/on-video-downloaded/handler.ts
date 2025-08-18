﻿import { fromJson } from './models';
import { SNSEvent } from 'aws-lambda';
import { process } from './processor';
import { retry } from '../../shared/helpers/retry';
import { setToken } from '../../api-clients/loan-api/src/loan-api-client';
import { config, initConfig } from '../../config/config';
import { client_setClientToken } from '@lightweight-clients/telegram-bot-api-lightweight-client';

const processMessage = async (message: string): Promise<void> => {
    console.log('Processing message: ', message);

    const videoDownloadedNotification = fromJson(message);
    await process(videoDownloadedNotification);

    console.log('Message processed');
};

export const handler = async (event: SNSEvent): Promise<void> => {
    console.log('Processing event: ', JSON.stringify(event));

    await initConfig();
    setToken(config.value.loanApi.token);
    client_setClientToken(config.value.telegram.token);

    for (const record of event.Records) {
        console.log('Processing record: ', record?.Sns?.MessageId);
        await retry(async () => await processMessage(record.Sns.Message), 3, () => true);
    }

    console.info('done');
};
