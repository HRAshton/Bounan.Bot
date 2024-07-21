// import { SubscriptionEntity } from '../../shared/entities/subscription-entity';
// import { AnimeKey } from '../../shared/entities/anime-key';
// import { GetCommand, UpdateCommand } from '@aws-sdk/lib-dynamodb';
// import { config } from '../../config/config';
// import { docClient, getAnimeKey } from '../../shared/repository';
//
// export type GetSubscriptionsResult = Pick<SubscriptionEntity, 'ChatIds' | 'OneTimeSubscribers'>;
//
// export const getSubscriptions = async (animeKey: AnimeKey): Promise<GetSubscriptionsResult> => {
//     const command = new GetCommand({
//         TableName: config.database.subscriptionsTableName,
//         Key: { AnimeKey: getAnimeKey(animeKey) },
//         ProjectionExpression: 'ChatIds, OneTimeSubscribers',
//     });
//
//     const result = await docClient.send(command);
//     console.log('Subscriptions: ' + JSON.stringify(result.Item));
//
//     return result.Item as GetSubscriptionsResult;
// }
//
// export const removeOneTimeSubscribers = async (animeKey: AnimeKey, episode: number): Promise<void> => {
//     const command = new UpdateCommand({
//         TableName: config.database.subscriptionsTableName,
//         Key: { AnimeKey: getAnimeKey(animeKey) },
//         UpdateExpression: 'REMOVE OneTimeSubscribers.#episode',
//         ExpressionAttributeNames: {
//             '#episode': episode.toString(),
//         },
//         ConditionExpression: 'attribute_exists(AnimeKey)',
//     });
//
//     const result = await docClient.send(command);
//     console.log('Removed one-time subscribers: ' + JSON.stringify(result));
// }