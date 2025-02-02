// {
//     const local_config = {
//         'alertEmail': '',
//         'loanApiToken': '',
//         'getAnimeFunctionName': '',
//         'videoDownloadedTopicArn': '',
//         'usersTableName': '',
//         'subscriptionsTableName': '',
//         'telegramBotToken': '',
//         'telegramBotVideoChatId': 0,
//         'telegramBotVideoChatId_dev': 0,
//         'telegramBotVideoChatId_prod': 0,
//         'telegramBotPublisherGroupName': '',
//         'studioLogosUrl': '',
//     };
//    
//     process.env.AWS_PROFILE = '';
//     process.env.ANIMAN_GET_ANIME_FUNCTION_NAME = local_config.getAnimeFunctionName;
//     process.env.LOAN_API_TOKEN = local_config.loanApiToken;
//     process.env.LOAN_API_MAX_CONCURRENT_REQUESTS = '6';
//     process.env.DATABASE_USERS_TABLE_NAME = local_config.usersTableName;
//     process.env.DATABASE_SUBSCRIPTIONS_TABLE_NAME = local_config.subscriptionsTableName;
//     process.env.TELEGRAM_TOKEN = local_config.telegramBotToken;
//     process.env.TELEGRAM_VIDEO_CHAT_ID = local_config.telegramBotVideoChatId.toString();
//     process.env.TELEGRAM_PUBLISHER_GROUP_NAME = local_config.telegramBotPublisherGroupName;
//     process.env.TELEGRAM_BUTTONS_COLUMNS = '7';
//     process.env.TELEGRAM_BUTTONS_ROWS = '3';
//     process.env.RETRY_MAX_ATTEMPTS = '1';
//     process.env.RETRY_DELAY_MS = '1000';
//     process.env.STUDIO_LOGOS_URL = local_config.studioLogosUrl;
// }

interface AniManConfig {
    getAnimeFunctionName: string;
}

interface LoanApiConfig {
    token: string;
    maxConcurrentRequests: number;
}

interface TelegramConfig {
    token: string;

    videoChatId: number;
    publisherGroupName: string;
}

interface RetryConfig {
    maxAttempts: number;
    delayMs: number;
}

interface AssetsConfig {
    studioLogosUrl: string;
}

interface DatabaseConfig {
    usersTableName: string;
    // subscriptionsTableName: string;
}

export interface Config {
    animan: AniManConfig;
    loanApi: LoanApiConfig;
    database: DatabaseConfig;
    telegram: TelegramConfig;
    retry: RetryConfig;
    assets: AssetsConfig;
}

const getEnv = (key: string): string => {
    const value = process.env[key];

    if (!value) {
        throw new Error(`Missing environment variable: ${key}`);
    }

    return value;
}

export const config: Config = {
    animan: {
        getAnimeFunctionName: getEnv('ANIMAN_GET_ANIME_FUNCTION_NAME'),
    },
    loanApi: {
        token: getEnv('LOAN_API_TOKEN'),
        maxConcurrentRequests: parseInt(getEnv('LOAN_API_MAX_CONCURRENT_REQUESTS')),
    },
    database: {
        usersTableName: getEnv('DATABASE_USERS_TABLE_NAME'),
        // subscriptionsTableName: getEnv('DATABASE_SUBSCRIPTIONS_TABLE_NAME'),
    },
    telegram: {
        token: getEnv('TELEGRAM_TOKEN'),

        videoChatId: parseInt(getEnv('TELEGRAM_VIDEO_CHAT_ID')),
        publisherGroupName: getEnv('TELEGRAM_PUBLISHER_GROUP_NAME'),
    },
    retry: {
        maxAttempts: parseInt(getEnv('RETRY_MAX_ATTEMPTS')),
        delayMs: parseInt(getEnv('RETRY_DELAY_MS')),
    },
    assets: {
        studioLogosUrl: getEnv('STUDIO_LOGOS_URL'),
    },
}