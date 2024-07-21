// process.env.AWS_PROFILE = '';
// process.env.ANIMAN_GET_ANIME_FUNCTION_NAME = '';
// process.env.LOAN_API_TOKEN = '';
// process.env.LOAN_API_MAX_CONCURRENT_REQUESTS = '';
// process.env.TELEGRAM_TOKEN = '';
// process.env.TELEGRAM_VIDEO_CHAT_ID = '';
// process.env.TELEGRAM_PUBLISHER_GROUP_NAME = '';
// process.env.TELEGRAM_BUTTONS_COLUMNS = '';
// process.env.TELEGRAM_BUTTONS_ROWS = '';
// process.env.RETRY_MAX_ATTEMPTS = '';
// process.env.RETRY_DELAY_MS = '';


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

    buttons: {
        columns: number;
        rows: number;
    };
}

interface RetryConfig {
    maxAttempts: number;
    delayMs: number;
}

// interface DatabaseConfig {
//     usersTableName: string;
//     subscriptionsTableName: string;
// }

export interface Config {
    animan: AniManConfig;
    loanApi: LoanApiConfig;
    telegram: TelegramConfig;
    retry: RetryConfig;
    // database: DatabaseConfig;
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
    telegram: {
        token: getEnv('TELEGRAM_TOKEN'),

        videoChatId: parseInt(getEnv('TELEGRAM_VIDEO_CHAT_ID')),
        publisherGroupName: getEnv('TELEGRAM_PUBLISHER_GROUP_NAME'),

        buttons: {
            columns: parseInt(getEnv('TELEGRAM_BUTTONS_COLUMNS')),
            rows: parseInt(getEnv('TELEGRAM_BUTTONS_ROWS')),
        },
    },
    retry: {
        maxAttempts: parseInt(getEnv('RETRY_MAX_ATTEMPTS')),
        delayMs: parseInt(getEnv('RETRY_DELAY_MS')),
    },
    // database: {
    //     usersTableName: getEnv('DATABASE_USERS_TABLE_NAME'),
    //     subscriptionsTableName: getEnv('DATABASE_SUBSCRIPTIONS_TABLE_NAME'),
    // },
}