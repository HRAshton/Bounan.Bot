import {
    VideoDownloadedNotification as RawVideoDownloadedNotification,
} from '../../api-clients/animan/common/ts/interfaces';
import { KeysToCamelCase, toCamelCase } from '../../shared/object-transformer';

export type VideoDownloadedNotification = KeysToCamelCase<RawVideoDownloadedNotification>;

export const fromJson = (jsonText: string): KeysToCamelCase<VideoDownloadedNotification> => {
    const json = JSON.parse(jsonText) as unknown as Partial<RawVideoDownloadedNotification>;
    const result = toCamelCase(json);

    if (!result) {
        throw new Error('Invalid JSON: ' + JSON.stringify(result));
    }

    if (!result.videoKey) {
        throw new Error('Invalid JSON: ' + JSON.stringify(result));
    }

    if (!Number.isInteger(result.videoKey.myAnimeListId)) {
        throw new Error('Invalid MyAnimeListId: ' + JSON.stringify(result));
    }

    if (typeof result.videoKey.dub !== 'string' || result.videoKey.dub.length === 0) {
        throw new Error('Invalid Dub: ' + JSON.stringify(result));
    }

    if (!Number.isInteger(result.videoKey.episode)) {
        throw new Error('Invalid Episode: ' + JSON.stringify(result));
    }

    if (result.messageId && !Number.isInteger(result.messageId)) {
        throw new Error('Invalid MessageId: ' + JSON.stringify(result));
    }

    return result as KeysToCamelCase<VideoDownloadedNotification>;
}
