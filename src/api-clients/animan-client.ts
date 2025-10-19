import { config } from '../config/config';
import { BotRequest, BotResponse } from '../shared/common/ts/interfaces';
import { makeLambdaRequest } from '../shared/common/ts/runtime/lambda-client';
import { VideoKey } from '../shared/database/entities/video-key';
import { assert } from '../shared/helpers/assert';

export const getVideoInfo = async (videoKey: VideoKey): Promise<BotResponse> => {
    const upperVideoKey: BotRequest = {
        videoKey: {
            myAnimeListId: videoKey.myAnimeListId,
            dub: videoKey.dub,
            episode: videoKey.episode,
        },
    }

    const result = await makeLambdaRequest<BotRequest, BotResponse>(
        config.value.animan.getAnimeFunctionName,
        upperVideoKey,
    );

    assert('status' in result);

    return result;
}