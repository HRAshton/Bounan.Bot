import { GetCommand } from '@aws-sdk/lib-dynamodb';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { getAnimeKey, getEpisodes } from './repository';

const { sendMock, getInputMock } = await vi.hoisted(
  () => import('../test/mocks/aws-sdk-lib-dynamodb-mock')
    .then(m => m.injectLibDynamoDbMocks()));

vi.mock('@aws-sdk/client-dynamodb', () => {
  class DynamoDBClient {
    constructor() {
    }
  }

  return { DynamoDBClient };
});

vi.mock('../config/config', () => {
  return {
    config: {
      value: {
        database: {
          tableName: 'anime-table',
        },
      },
    },
  };
});

describe('repository', () => {
  beforeEach(() => {
    sendMock.mockReset();
    getInputMock.mockReset();
  });

  describe('getAnimeKey', () => {
    it('builds a composite anime key using myAnimeListId and dub flag', () => {
      const key = getAnimeKey({ myAnimeListId: 123, dub: 'true' });
      expect(key).toBe('123#true');
    });
  });

  describe('getEpisodes', () => {
    it('queries DynamoDB with the correct table name, key, and attributes', async () => {
      sendMock.mockResolvedValueOnce({
        Item: {
          episodes: [1, 2, 3],
          updatedAt: '2026-01-24T12:00:00Z',
        },
      });

      const result = await getEpisodes({ myAnimeListId: 999, dub: 'false' });

      expect(getInputMock).toHaveBeenCalledTimes(1);
      expect(getInputMock).toHaveBeenCalledWith({
        TableName: 'anime-table',
        Key: { animeKey: '999#false' },
        AttributesToGet: ['episodes', 'updatedAt'],
      });

      expect(sendMock).toHaveBeenCalledTimes(1);

      const sentCommand = sendMock.mock.calls[0][0];
      expect(sentCommand).toBeInstanceOf(GetCommand);

      expect(result).toEqual({
        episodes: [1, 2, 3],
        updatedAt: '2026-01-24T12:00:00Z',
      });
    });

    it('returns undefined fields when Item is missing attributes', async () => {
      sendMock.mockResolvedValueOnce({ Item: {} });

      const result = await getEpisodes({ myAnimeListId: 1, dub: 'false' });

      expect(result).toEqual({});
    });

    it('propagates DynamoDB errors', async () => {
      sendMock.mockRejectedValueOnce(new Error('ddb down'));

      await expect(
        getEpisodes({ myAnimeListId: 1, dub: 'true' }),
      ).rejects.toThrow('ddb down');

      expect(sendMock).toHaveBeenCalledTimes(1);
    });
  });
});
