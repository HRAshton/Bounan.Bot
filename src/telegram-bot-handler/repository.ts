import { PutCommand } from '@aws-sdk/lib-dynamodb';
import { config } from '../config/config';
import { docClient } from '../shared/database/repository';
import { UserEntity } from '../shared/database/entities/user-entity';

export const registerNewUserIfNotExists = async (userId: number): Promise<void> => {
    const command = new PutCommand({
        TableName: config.database.usersTableName,
        Item: {
            userId,
            directRank: 0,
            indirectRank: 0,
            requestedEpisodes: [],
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString(),
        } as UserEntity,
        ConditionExpression: 'attribute_not_exists(userId)',
    });

    let result: string;
    try {
        const response = await docClient.send(command);
        result = JSON.stringify(response);
    } catch (error) {
        if (error instanceof Error && error.name === 'ConditionalCheckFailedException') {
            result = 'User already exists';
        } else {
            throw error;
        }
    }

    console.log(`Register user result: ${result}`);
}