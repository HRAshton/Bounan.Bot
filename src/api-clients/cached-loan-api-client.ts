import { asyncMemoized } from '../../third-party/common/ts/runtime/memorized';
import * as orig from '../../third-party/loan-api/src/loan-api-client';

export * from '../../third-party/loan-api/src/loan-api-client';

export const getAllExistingVideos = asyncMemoized('getAllExistingVideos', orig.getAllExistingVideos);

export const getDubs = asyncMemoized('getDub', orig.getDubs);