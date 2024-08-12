export interface ShikiAnimeInfo {
    id: number;
    name: string;
    url: string;
    image: {
        original: string;
        preview: string;
    };

    russian?: string;
    english?: string[];
    synonyms?: string[];
    license_name_ru?: string;

    episodes?: number;
    episodes_aired?: number;
    franchise?: string;
    aired_on?: string;
    genres?: { russian: string; }[];
}

