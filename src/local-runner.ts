/* eslint @typescript-eslint/no-explicit-any: 0 */

import { handler as onVideoDownloadedHandler } from './handlers/on-video-downloaded/handler';
import { VideoDownloadedNotification } from './api-clients/animan/common/ts/interfaces';
import { handler as onWebhookHandler } from './handlers/on-webhook/handler';
import { Update } from 'telegram-bot-api-lightweight-client';
import { config } from './config/config';

const onDownloaded = async (message: VideoDownloadedNotification) => {
    console.log('Processing message: ', message);

    // @ts-expect-error - we don't need to provide all the event properties
    await onVideoDownloadedHandler({ Records: [{ Sns: { Message: JSON.stringify(message) } }] });

    console.log('Message processed');
}

const onWebhook = async (message: Update) => {
    // @ts-expect-error - we don't need to provide all the event properties
    await onWebhookHandler({ body: JSON.stringify(message) });
}

const main = async () => {
    // await onDownloaded({
    //     VideoKey: {
    //         MyAnimeListId: 1165,
    //         Dub: 'Субтитры',
    //         Episode: 1,
    //     },
    //     MessageId: 121,
    //     SubscriberChatIds: [442033576],
    //     Scenes: {
    //         Opening: {
    //             Start: 0,
    //             End: 100,
    //         },
    //     },
    //     PublishingDetails: {
    //         MessageId: 4329,
    //         ThreadId: 4241,
    //     },
    // });
    
    await onDownloaded({
        "VideoKey": {
            "MyAnimeListId": 6919,
            "Dub": "AniDUB",
            "Episode": 12
        },
        "SubscriberChatIds": [
            442033576
        ]
    });
}

const pooling = async () => {
    let offset = 0;
    while (true) {
        const result = await fetch(`https://api.telegram.org/bot${config.telegram.token}/getUpdates?offset=${offset}&timeout=60`);
        const updates = await result.json();
        offset = updates.result[updates.result.length - 1]?.update_id + 1 || offset;

        for (const update of updates.result) {
            await onWebhook(update);
        }

        await new Promise((resolve) => setTimeout(resolve, 1000));
    }
}

main();
// pooling();
