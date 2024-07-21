import { APIGatewayEvent, APIGatewayProxyResultV2 } from 'aws-lambda';
import { Texts } from '../../shared/telegram/texts';
import { config } from '../../config/config';
import { retry } from '../../shared/helpers/retry';
import { setToken } from '../../api-clients/loan-api/src/animan-loan-api-client';
import { CallbackQuery } from 'telegram-bot-api-lightweight-client/src/types';
import { client_setClientToken } from 'telegram-bot-api-lightweight-client/src/core';
import { BotSettings, handleUpdate } from '../../telegram-bot-handler/update-handler';
import { dubsInlineQueryHandler } from './event-handlers/inline-query-handlers/dubs-inline-query-handler';
import { infoCallbackQueryHandler } from './event-handlers/callback-query-adapters/info-callback-query-handler';
import { infoMessageHandler } from './event-handlers/message-handlers/info-message-handler';
import { knownInlineAnswerMessageHandler } from './event-handlers/message-handlers/known-inline-answer-message-handler';
import { relatedInlineQueryHandler } from './event-handlers/inline-query-handlers/related-inline-query-handler';
import { searchInlineQueryHandler } from './event-handlers/inline-query-handlers/search-inline-query';
import { searchMessageHandler } from './event-handlers/message-handlers/search-message-handler';
import { startMessageHandler } from './event-handlers/message-handlers/start-message-handler';
import { watchCallbackQueryHandler } from './event-handlers/callback-query-adapters/watch-callback-query-handler';
import { watchMessageHandler } from './event-handlers/message-handlers/watch-message-handler';

const settings: BotSettings = {
    onMessage: [
        startMessageHandler,
        knownInlineAnswerMessageHandler,
        infoMessageHandler,
        watchMessageHandler,
    ],
    onMessageDefault: searchMessageHandler.handler,

    onInlineQuery: [
        dubsInlineQueryHandler,
        relatedInlineQueryHandler,
    ],
    onInlineQueryDefault: searchInlineQueryHandler.handler,

    onCallbackQuery: [
        infoCallbackQueryHandler,
        watchCallbackQueryHandler,
    ],
    onCallbackQueryDefault: async (callbackQuery: CallbackQuery) => {
        console.error('Received unknown callback query', callbackQuery);
        return {
            callback_query_id: callbackQuery.id,
            show_alert: true,
            text: Texts.UnknownError,
        };
    },
};

setToken(config.loanApi.token);
client_setClientToken(config.telegram.token);

const SUCCESS_RESPONSE = { statusCode: 202 };

export const handler = async (event: APIGatewayEvent): Promise<APIGatewayProxyResultV2> => {
    try {
        if (!event.body) {
            console.log('No body');
            return SUCCESS_RESPONSE;
        }

        const update = JSON.parse(event.body);
        await retry(async () => await handleUpdate(update, settings), 3, () => true);
    } catch (error: unknown) {
        console.error('Failed to process update', error);
    }

    return SUCCESS_RESPONSE;
};
