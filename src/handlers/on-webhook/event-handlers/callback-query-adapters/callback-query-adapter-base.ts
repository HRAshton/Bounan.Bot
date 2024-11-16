import { assert } from '../../../../shared/helpers/assert';
import { CallbackQuery } from 'telegram-bot-api-lightweight-client';
import { MessageHandler } from '../query-handler';
import { AnswerCallbackQueryData } from 'telegram-bot-api-lightweight-client';

export const processCallbackQuery = async (
    callbackQuery: CallbackQuery,
    handler: MessageHandler,
): Promise<Omit<AnswerCallbackQueryData, 'callback_query_id'>> => {
    assert(!!callbackQuery?.id);
    assert(!!callbackQuery?.data);
    assert(!!callbackQuery?.message?.chat);
    assert(!!callbackQuery?.message?.message_id);

    console.log('Handling info callback query');

    await handler({
        chat: callbackQuery.message!.chat,
        text: callbackQuery.data,
        message_id: callbackQuery.message!.message_id,
    });

    return {};
}