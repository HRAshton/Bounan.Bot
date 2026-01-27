import type { UserStatus } from './user-status';
import type { VideoKey } from './video-key';

export interface UserEntity {
  userId: number;

  directRank: number;
  indirectRank: number;
  requestedEpisodes: VideoKey[];

  status: UserStatus;

  createdAt: string;
  updatedAt: string;
}
