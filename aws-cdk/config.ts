import configFile from './configuration.json';

interface Config {
    alertEmail: string;
    loanApiToken: string;
    telegramBotToken: string;
    telegramBotVideoChatId: number;
    telegramBotPublisherGroupName: string;
    getAnimeFunctionName: string;
    videoDownloadedTopicArn: string;
}

export const config: Config = configFile;

if (!config.alertEmail) {
    throw new Error('errorAlarmEmail is required');
}
if (!config.loanApiToken) {
    throw new Error('loanApiToken is required');
}
if (!config.telegramBotToken) {
    throw new Error('telegramBotToken is required');
}
if (!config.telegramBotVideoChatId) {
    throw new Error('telegramBotVideoChatId is required');
}
if (!config.telegramBotPublisherGroupName) {
    throw new Error('telegramBotPublisherGroupName is required');
}
if (!config.getAnimeFunctionName) {
    throw new Error('getAnimeFunctionName is required');
}
if (!config.videoDownloadedTopicArn) {
    throw new Error('videoDownloadedTopicArn is required');
}
