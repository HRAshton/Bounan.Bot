import { asyncMemoized } from '../shared/common/ts/runtime/memorized';
import * as orig from './shikimori/shikimori-client';

export * from './shikimori/shikimori-client';

export const getShikiAnimeInfo = asyncMemoized('getShikiAnimeInfo', orig.getShikiAnimeInfo);

export const getRelated = asyncMemoized('getRelated', orig.getRelated);

export const searchAnime = asyncMemoized('searchAnime', orig.searchAnime);