import {
    AnswerCallbackQueryData,
    CallbackQuery,
    InlineQuery,
    InlineQueryResult,
    Message,
} from '@lightweight-clients/telegram-bot-api-lightweight-client';

export type QueryHandler<TUpdateItem, TResult> = (updateItem: TUpdateItem) => Promise<TResult>;

export type CallbackQueryHandler = QueryHandler<CallbackQuery, Omit<AnswerCallbackQueryData, 'callback_query_id'>>;

export type InlineQueryHandler = QueryHandler<InlineQuery, InlineQueryResult[]>;

export type MessageHandler = QueryHandler<Pick<Message, 'text' | 'chat' | 'message_id'>, void>;
