﻿import { ShikiAnimeInfo } from '../../api-clients/shikimori/shiki-anime-info';
import { Scenes, VideoKey } from '../models';
import { Texts } from './texts';

const escapeLinks = (text: string): string => {
    return text.replaceAll('.', '');
}

const pad = (num: number) => num.toString().padStart(2, '0');

const secToTime = (sec: number): string => {
    const minutes = Math.floor(sec / 60);
    const seconds = Math.floor(sec - minutes * 60);
    return `${pad(minutes)}:${pad(seconds)}`;
}

export const getVideoDescription = (
    animeInfo: ShikiAnimeInfo,
    videoKey: VideoKey,
    scenes: Scenes | undefined,
): string => {
    const has_episodes = animeInfo.episodes && animeInfo.episodes > 1
        || animeInfo.episodes_aired && animeInfo.episodes_aired > 1;

    return [
        Texts.VideoDescription__Name
            .replace('%1', animeInfo.russian || animeInfo.name)
            .replace('%2', videoKey.dub && `(${escapeLinks(videoKey.dub)})`),

        has_episodes
        && Texts.VideoDescription__Episode.replace('%1', videoKey.episode.toString()),

        scenes?.opening
        && Texts.VideoDescription__EndOfOpening
            .replace('%1', secToTime(scenes.opening.end))
            .replace('%2', secToTime(scenes.opening.start)),

        scenes?.sceneAfterEnding
        && Texts.VideoDescription__SceneAfterEnding.replace('%1', secToTime(scenes.sceneAfterEnding.start)),
    ]
        .filter(Boolean)
        .join('\n');
}
