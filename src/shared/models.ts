import {
    VideoKey as RawVideoKey,
    Scenes as RawScenes,
    PublishingDetails as RawPublishingDetails,
    BotResponse as RawBotResponse,
} from '../api-clients/animan/common/ts/interfaces';
import { KeysToCamelCase } from './object-transformer';

export type VideoKey = KeysToCamelCase<RawVideoKey>;
export type Scenes = KeysToCamelCase<RawScenes>;
export type PublishingDetails = KeysToCamelCase<RawPublishingDetails>;
export type BotResponse = KeysToCamelCase<RawBotResponse>;