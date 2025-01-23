import { UserStatus } from './user-status';
import { VideoKey } from './video-key';

export interface UserEntity {
    userId: number;

    directRank: number;
    indirectRank: number;
    requestedEpisodes: VideoKey[];

    status: UserStatus;

    createdAt: string;
    updatedAt: string;
}
