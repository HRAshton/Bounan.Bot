import { InvokeCommand, LambdaClient } from '@aws-sdk/client-lambda';

import { config } from '../../config/config';
import { assert } from '../../shared/helpers/assert';
import { BotResponse, VideoKey } from '../../shared/models';
import { toCamelCase } from '../../shared/object-transformer';
import { BotRequest as RawBotRequest } from './common/ts/interfaces';

const lambdaClient = new LambdaClient({});

export const getVideoInfo = async (videoKey: VideoKey): Promise<BotResponse> => {
    console.log('Getting anime for video key: ', videoKey);

    const upperVideoKey: RawBotRequest = {
        VideoKey: {
            MyAnimeListId: videoKey.myAnimeListId,
            Dub: videoKey.dub,
            Episode: videoKey.episode,
        },
        ChatId: -1,
    }

    const message = JSON.stringify(upperVideoKey);
    console.log('Sending request: ', message);

    const response = await lambdaClient.send(new InvokeCommand({
        FunctionName: config.value.animan.getAnimeFunctionName,
        Payload: message,
    }));
    console.log('Request sent: ', response);

    const rawResult = JSON.parse(Buffer.from(response.Payload!).toString());
    console.log('Received response: ', rawResult);

    const result: BotResponse = toCamelCase<BotResponse>(rawResult);
    assert('status' in result);

    return result;
}