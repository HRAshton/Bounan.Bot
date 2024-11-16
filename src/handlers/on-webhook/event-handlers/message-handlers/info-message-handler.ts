import { sendMessage, sendPhoto, Message } from 'telegram-bot-api-lightweight-client';
import { InfoCommandDto, DubsCommandDto, RelatedCommandDto } from '../../command-dtos';
import { assert } from '../../../../shared/helpers/assert';
import { MessageHandler } from '../query-handler';
import { getAnimeInfo, toAbsoluteUrl } from '../../../../api-clients/shikimori/shikimori-client';
import { Texts } from '../../../../shared/telegram/texts';

const canHandle = (message: Message): boolean => message.text?.startsWith(InfoCommandDto.Command) ?? false;

const handler: MessageHandler = async (message) => {
    assert(!!message.text);
    assert(!!message.chat?.id);

    console.log('Handling info message: ', message.text);

    const commandDto = InfoCommandDto.fromPayload(message.text!) as InfoCommandDto;
    console.log('Parsed command: ', JSON.stringify(commandDto));
    if (!commandDto) {
        console.warn('Failed to deserialize command', message.text);
        return;
    }

    const anime = await getAnimeInfo(commandDto.myAnimeListId);
    console.log('Anime info: ', JSON.stringify(anime));
    if (!anime) {
        console.error('Anime not found');
        await sendMessage({
            chat_id: message.chat.id,
            text: Texts.Search__NoResultsInLoan,
        });
        return;
    }

    const caption = [
        Texts.AnimeDescription__Name.replaceAll('%1', anime.russian || anime.name),
        anime.aired_on && Texts.AnimeDescription__AiredOn.replaceAll('%1', anime.aired_on.substring(0, 4)),
        anime.genres && Texts.AnimeDescription__Genres.replaceAll('%1', anime.genres.map((g) => g.russian).join(', ')),
        Texts.AnimeDescription__Links.replaceAll('%1', anime.id.toString()),
    ]
        .filter(Boolean)
        .join('\n');

    const result = await sendPhoto({
        chat_id: message.chat.id,
        photo: toAbsoluteUrl(anime.image.original),
        caption: caption,
        parse_mode: 'HTML',
        reply_markup: {
            inline_keyboard: [
                [
                    {
                        text: Texts.Button__Watch,
                        switch_inline_query_current_chat: new DubsCommandDto(anime.id).toString(),
                    },
                    {
                        text: Texts.Button__Franchise,
                        switch_inline_query_current_chat: new RelatedCommandDto(anime.id).toString(),
                    },
                ],
            ],
        },
    });
    assert(result.ok, () => JSON.stringify(result));

    console.log('Info message sent');
};

export const infoMessageHandler = {
    canHandle,
    handler,
};