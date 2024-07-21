import { ShikiAnimeInfo } from './shiki-anime-info';
import { ShikiRelated } from './shiki-related';

let rateLimitingPromise: Promise<void> = Promise.resolve();

export const SHIKIMORI_BASE_URL = 'https://shikimori.one';

const request = async <T>(relativeUrl: string): Promise<T> => {
    await rateLimitingPromise;
    rateLimitingPromise = new Promise((resolve) => setTimeout(resolve, 100));

    const response = await fetch(SHIKIMORI_BASE_URL + relativeUrl, {
        headers: {
            'Content-Type': 'application/json',
            'User-Agent': 'Bounan.Bot',
        },
    });

    return await response.json();
}

export const searchAnime = async (query: string): Promise<ShikiAnimeInfo[]> => {
    return request<ShikiAnimeInfo[]>(`/api/animes?search=${encodeURIComponent(query)}&limit=10&censored=false&status=ongoing,released`);
}

export const getAnimeInfo = async (myAnimeListId: number): Promise<ShikiAnimeInfo> => {
    return request<ShikiAnimeInfo>(`/api/animes/${myAnimeListId}`);
}

export const getRelated = async (myAnimeListId: number): Promise<ShikiRelated[]> => {
    return request<ShikiRelated[]>(`/api/animes/${myAnimeListId}/related`);
}

export const toAbsoluteUrl = (relativeUrl: string) => SHIKIMORI_BASE_URL + relativeUrl;