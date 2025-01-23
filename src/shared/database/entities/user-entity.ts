import { VideoKey } from '../../models';

export interface UserEntity {
    userId: number;

    directRank: number;
    indirectRank: number;
    requestedEpisodes: VideoKey[];

    createdAt: string;
    updatedAt: string;
}
