import { InlineQuery, InlineQueryResultArticle } from 'telegram-bot-api-lightweight-client';
import { DubsCommandDto, WatchCommandDto } from '../../command-dtos';
import { InlineQueryHandler } from '../query-handler';
import { dubToKey } from '../../../../shared/helpers/dub-to-key';
import { getDubs } from '../../../../api-clients/loan-api/src/animan-loan-api-client';
import { KnownInlineAnswers } from '../../constants/known-inline-answers';
import { getStudioLogoUrl } from '../../../../shared/studio-logos-provider/studio-logos-provider';

const canHandle = (inlineQuery: InlineQuery): boolean => inlineQuery.query?.startsWith(DubsCommandDto.Command) ?? false;

const handler: InlineQueryHandler = async (inlineQuery) => {
    console.log('Handling dubs inline query {Query}', inlineQuery.query);

    const commandDto = DubsCommandDto.fromPayload(inlineQuery.query) as DubsCommandDto;
    if (!commandDto) {
        console.warn('Failed to deserialize command', inlineQuery.query);
        return [];
    }

    const uniqueDubs = await getDubs(commandDto.myAnimeListId);
    console.log('Got dubs for {MyAnimeListId}: {Dubs}', commandDto.myAnimeListId, uniqueDubs);
    if (uniqueDubs.length === 0) {
        return [{
            type: 'article',
            id: KnownInlineAnswers.AnimeUnavailable,
            title: KnownInlineAnswers.AnimeUnavailable,
            input_message_content: {
                message_text: KnownInlineAnswers.AnimeUnavailable,
            },
        }];
    }

    const results: InlineQueryResultArticle[] = uniqueDubs
        .sort((a, b) => a.name.localeCompare(b.name))
        .map(item => ({
            type: 'article',
            id: item.name,
            title: item.name,
            thumbnail_url: getStudioLogoUrl(item.name),
            input_message_content: {
                message_text: new WatchCommandDto(
                    commandDto.myAnimeListId,
                    dubToKey(item.name),
                    item.firstEpisode,
                ).toString(),
            },
        }));

    console.log('Returning {Count} dubs for {MyAnimeListId}', results.length, commandDto.myAnimeListId);
    return results;
}

export const dubsInlineQueryHandler = {
    canHandle,
    handler,
};
