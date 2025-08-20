import { InvokeCommand, LambdaClient } from '@aws-sdk/client-lambda';

import { config } from '../../config/config';
import { VideoKey } from '../../shared/database/entities/video-key';
import { assert } from '../../shared/helpers/assert';
import { BotRequest, BotResponse } from './common/ts/interfaces';

const lambdaClient = new LambdaClient({});

export const getVideoInfo = async (videoKey: VideoKey): Promise<BotResponse> => {
    console.log('Getting anime for video key: ', videoKey);

    const upperVideoKey: BotRequest = {
        videoKey: {
            myAnimeListId: videoKey.myAnimeListId,
            dub: videoKey.dub,
            episode: videoKey.episode,
        },
    }

    const message = JSON.stringify(upperVideoKey);
    console.log('Sending request: ', message);

    const response = await lambdaClient.send(new InvokeCommand({
        FunctionName: config.value.animan.getAnimeFunctionName,
        Payload: message,
    }));
    console.log('Request sent: ', response);

    const result: BotResponse = JSON.parse(Buffer.from(response.Payload!).toString());
    console.log('Received response: ', result);

    assert('status' in result);

    return result;
}