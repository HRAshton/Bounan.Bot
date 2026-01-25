export interface AnimeEntity {
    animeKey: string;
    myAnimeListId: number;
    dub: string;

    episodes: Set<number>;

    createdAt: string;
    updatedAt: string;
}

export type AnimeKey = Pick<AnimeEntity, 'myAnimeListId' | 'dub'>;