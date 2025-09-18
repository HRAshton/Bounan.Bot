import { asyncMemoized } from '../shared/common/ts/runtime/memorized';
import * as orig from './loan-api/src/loan-api-client';

export * from './loan-api/src/loan-api-client';

export const getAllExistingVideos = asyncMemoized('getAllExistingVideos', orig.getAllExistingVideos);

export const getDubs = asyncMemoized('getDub', orig.getDubs);