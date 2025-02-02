import { fetchSsmValue } from '../api-clients/animan/common/ts/runtime/ssm-client';

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

let cachedConfig: Config | undefined;

export const initConfig = async (): Promise<void> => {
    cachedConfig = await fetchSsmValue('/bounan/bot/runtime-config') as Config;
}

export const config = {
    get value() {
        if (!cachedConfig) {
            throw new Error('Config not initialized');
        }

        return cachedConfig;
    },
}