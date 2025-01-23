import {
    AnswerCallbackQueryData,
    CallbackQuery,
    InlineQuery,
    Message,
    Update,
} from 'telegram-bot-api-lightweight-client';
import { answerCallbackQuery, answerInlineQuery, InlineQueryResult } from 'telegram-bot-api-lightweight-client';
import { getUserStatus, registerNewUserIfNotExists } from './repository';
import { UserStatus } from '../shared/database/entities/user-status';

type CanHandleUpdate<TUpdateField> = (updateField: TUpdateField) => boolean;
type Handler<TUpdateField, TResult> = (updateField: TUpdateField) => Promise<TResult>;

export interface CanHandleUpdatePair<TUpdateField, TResult> {
    canHandle: CanHandleUpdate<TUpdateField>;
    handler: Handler<TUpdateField, TResult>;
}

export interface BotSettings {
    onMessage: CanHandleUpdatePair<Message, void>[];
    onMessageDefault: Handler<Message, void> | undefined;

    onCallbackQuery: CanHandleUpdatePair<CallbackQuery, Omit<AnswerCallbackQueryData, 'callback_query_id'>>[];
    onCallbackQueryDefault: Handler<CallbackQuery, Omit<AnswerCallbackQueryData, 'callback_query_id'>> | undefined;

    onInlineQuery: CanHandleUpdatePair<InlineQuery, InlineQueryResult[]>[];
    onInlineQueryDefault: Handler<InlineQuery, InlineQueryResult[]> | undefined;
}

const ensureUserExistsAndGetStatus = async (userId: number | undefined): Promise<UserStatus> => {
    if (!userId) {
        throw new Error('User ID is required');
    }

    await registerNewUserIfNotExists(userId);

    return getUserStatus(userId);
}

const tryHandleUpdate = async <TUpdateField, TResult>(
    updateField: TUpdateField,
    handlers: CanHandleUpdatePair<TUpdateField, TResult>[],
    defaultHandler?: Handler<TUpdateField, TResult>,
): Promise<TResult | null> => {
    const handler = handlers.find(({ canHandle }) => canHandle(updateField))?.handler;
    if (handler) {
        return await handler(updateField);
    } else if (defaultHandler) {
        return await defaultHandler(updateField);
    } else {
        return null;
    }
}

export const handleUpdate = async (update: Update, settings: BotSettings): Promise<void> => {
    console.log('Processing update: ', update);

    if (update.message) {
        const userStatus = await ensureUserExistsAndGetStatus(update.message.from?.id);
        if (userStatus === UserStatus.SUSPENDED) {
            console.warn('User is suspended');
            return;
        }

        await tryHandleUpdate(update.message, settings.onMessage, settings.onMessageDefault);
    } else if (update.callback_query) {
        const userStatus = await ensureUserExistsAndGetStatus(update.callback_query.from.id);
        if (userStatus === UserStatus.SUSPENDED) {
            console.warn('User is suspended');
            return;
        }

        const result = await tryHandleUpdate<CallbackQuery, Omit<AnswerCallbackQueryData, 'callback_query_id'>>(
            update.callback_query,
            settings.onCallbackQuery,
            settings.onCallbackQueryDefault);
        console.log('Callback query processed', JSON.stringify(result));

        if (result) {
            const response = await answerCallbackQuery({
                ...result,
                callback_query_id: update.callback_query.id,
            });
            console.log('Answered callback query', JSON.stringify(response));
        }
    } else if (update.inline_query) {
        const userStatus = await ensureUserExistsAndGetStatus(update.inline_query.from.id);
        if (userStatus === UserStatus.SUSPENDED) {
            console.warn('User is suspended');
            return;
        }

        const results = await tryHandleUpdate<InlineQuery, InlineQueryResult[]>(
            update.inline_query,
            settings.onInlineQuery,
            settings.onInlineQueryDefault);
        console.log('Inline query processed', results);

        if (results) {
            const response = await answerInlineQuery({
                inline_query_id: update.inline_query.id,
                results,
            });
            console.log('Answered inline query', response);
        }
    }

    console.log('Update processed');
}