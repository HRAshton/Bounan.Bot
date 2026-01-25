import { DeleteCommand, DynamoDBDocumentClient, ScanCommand } from '@aws-sdk/lib-dynamodb';
import { mockClient } from 'aws-sdk-client-mock';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { deleteAnime, getAll } from './repository';

const docClientMock = mockClient(DynamoDBDocumentClient);
const getAnimeKeyMock = vi.hoisted(() => vi.fn());

vi.mock('../../shared/repository', async () => {
  const actual = await vi.importActual('../../shared/repository');
  return {
    ...actual,
    getAnimeKey: getAnimeKeyMock,
  } as typeof actual & { getAnimeKey: typeof getAnimeKeyMock };
});

vi.mock('../../config/config', () => ({
  config: {
    value: {
      database: {
        tableName: 'anime-table',
      },
    },
  },
}));

describe('repository', () => {
  beforeEach(() => {
    docClientMock.reset();
    getAnimeKeyMock.mockReset();

    vi.spyOn(console, 'log').mockImplementation(() => undefined);
  });

  describe('getAll', () => {
    it('scans the table and returns anime entities', async () => {
      const items = [{ animeKey: '1#false' }, { animeKey: '2#true' }];
      docClientMock.on(ScanCommand).resolves({ Items: items });

      const result = await getAll();

      // Assert a ScanCommand was sent with the expected input
      expect(docClientMock.calls()).toHaveLength(1);
      const sent = docClientMock.calls()[0].args[0] as ScanCommand;
      expect(sent).toBeInstanceOf(ScanCommand);
      expect(sent.input).toEqual({ TableName: 'anime-table' });

      expect(result).toEqual(items);
    });

    it('returns an empty array when no items exist', async () => {
      docClientMock.on(ScanCommand).resolves({ Items: [] });

      const result = await getAll();
      expect(result).toEqual([]);
    });

    it('propagates DynamoDB errors', async () => {
      docClientMock.on(ScanCommand).rejects(new Error('ddb down'));

      await expect(getAll()).rejects.toThrow('ddb down');
    });
  });

  describe('deleteAnime', () => {
    it('deletes an anime by computed animeKey', async () => {
      getAnimeKeyMock.mockReturnValueOnce('123#true');
      docClientMock.on(DeleteCommand).resolves({});

      await deleteAnime({ myAnimeListId: 123, dub: 'true' });

      expect(getAnimeKeyMock).toHaveBeenCalledWith({
        myAnimeListId: 123,
        dub: 'true',
      });

      // Assert DeleteCommand was created with the expected input
      expect(docClientMock.calls()).toHaveLength(1);
      const sent = docClientMock.calls()[0].args[0] as DeleteCommand;
      expect(sent).toBeInstanceOf(DeleteCommand);
      expect(sent.input).toEqual({
        TableName: 'anime-table',
        Key: { animeKey: '123#true' },
        ConditionExpression: 'attribute_exists(animeKey)',
      });
    });

    it('propagates DynamoDB errors', async () => {
      getAnimeKeyMock.mockReturnValueOnce('1#false');
      docClientMock.on(DeleteCommand).rejects(new Error('ddb down'));

      await expect(
        deleteAnime({ myAnimeListId: 1, dub: 'false' }),
      ).rejects.toThrow('ddb down');
    });
  });
});
