import { DynamoDBClient } from '@aws-sdk/client-dynamodb';
import { DynamoDBDocumentClient, GetCommand } from '@aws-sdk/lib-dynamodb';

import { config } from '../config/config';
import type { AnimeEntity, AnimeKey } from '../models/anime-entity';

const dynamoDbClient = new DynamoDBClient();

export const docClient = DynamoDBDocumentClient.from(dynamoDbClient);

export const getAnimeKey = (animeKey: AnimeKey): string => {
  return `${animeKey.myAnimeListId}#${animeKey.dub}`;
}

export const getEpisodes = async (animeKey: AnimeKey): Promise<Pick<AnimeEntity, 'episodes' | 'updatedAt'>> => {
  const command = new GetCommand({
    TableName: config.value.database.tableName,
    Key: { animeKey: getAnimeKey(animeKey) },
    AttributesToGet: ['episodes', 'updatedAt'],
  });

  const response = await docClient.send(command);
  return response.Item as Pick<AnimeEntity, 'episodes' | 'updatedAt'>;
}
