import { MessageHandler } from '../query-handler';
import { Message } from 'telegram-bot-api-lightweight-client/src/types';
import { deleteMessage } from 'telegram-bot-api-lightweight-client/src/client';
import { assert } from '../../../../shared/helpers/assert';
import { KnownInlineAnswers } from '../../constants/known-inline-answers';

const canHandle = (message: Message): boolean =>
    Object.values(KnownInlineAnswers).includes(message.text as KnownInlineAnswers);

const handler: MessageHandler = async (message) => {
    assert(!!message?.chat?.id);
    assert(!!message?.text);

    console.log('Handling known inline answer: ', message.text);

    const result = await deleteMessage({
        chat_id: message.chat.id,
        message_id: message.message_id,
    });
    assert(result.ok, () => JSON.stringify(result));
};

export const knownInlineAnswerMessageHandler = {
    canHandle,
    handler,
};