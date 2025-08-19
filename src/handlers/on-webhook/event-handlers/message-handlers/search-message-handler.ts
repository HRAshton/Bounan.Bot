import { MessageHandler } from '../query-handler';
import { sendMessage } from '@lightweight-clients/telegram-bot-api-lightweight-client';
import { assert } from '../../../../shared/helpers/assert';
import { searchAnime } from '../../../../api-clients/cached-shikimori-client';
import { InfoCommandDto } from '../../command-dtos';
import { eclipseText } from '../../../../shared/helpers/string-helpers';
import { Texts } from '../../../../shared/telegram/texts';

const canHandle = (): boolean => {
    throw new Error('Not supported');
};

const handler: MessageHandler = async (message) => {
    assert(!!message?.chat?.id);
    if (!message?.text?.trim()) {
        console.log('Empty search query. Message from group? Ignoring');
        return;
    }

    console.log('Handling search query: ', message.text);

    const searchResults = await searchAnime(message.text!);
    if (!searchResults || searchResults.length === 0) {
        console.log('No search results');
        await sendMessage({
            chat_id: message.chat.id,
            text: Texts.Search__NoResultsInShiki,
        });
        return;
    }

    console.log(`Found ${searchResults.length} results`);

    const buttons = searchResults.map((result) => ({
        text: eclipseText(result.russian || result.name, 25, 10),
        callback_data: new InfoCommandDto(result.id).toString(),
    }));

    const result = await sendMessage({
        chat_id: message.chat.id,
        text: Texts.FullMessageSearch__BestResults,
        reply_markup: {
            inline_keyboard: buttons.map((b) => [b]),
        },
    });
    assert(result.ok, () => JSON.stringify(result));
};

export const searchMessageHandler = {
    canHandle,
    handler,
};