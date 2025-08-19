import { asyncMemoized } from './animan/common/ts/runtime/memorized';
import * as orig from './loan-api/src/loan-api-client';

export * from './loan-api/src/loan-api-client';

export const getExistingVideos = asyncMemoized('getAllExistingVideos', orig.getExistingVideos);

export const getAllExistingVideos = asyncMemoized('getAllExistingVideos', orig.getAllExistingVideos);

export const getDubs = asyncMemoized('getDub', orig.getDubs);