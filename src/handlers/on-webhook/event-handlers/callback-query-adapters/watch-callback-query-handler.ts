import { CallbackQuery } from 'telegram-bot-api-lightweight-client';
import { CallbackQueryHandler } from '../query-handler';
import { processCallbackQuery } from './callback-query-adapter-base';
import { WatchCommandDto } from '../../command-dtos';
import { watchMessageHandler } from '../message-handlers/watch-message-handler';

const canHandle = (callbackQuery: CallbackQuery) => callbackQuery.data?.startsWith(WatchCommandDto.Command) || false;

const handler: CallbackQueryHandler = async (callbackQuery) =>
    await processCallbackQuery(callbackQuery, watchMessageHandler.handler);

export const watchCallbackQueryHandler = {
    canHandle,
    handler,
}