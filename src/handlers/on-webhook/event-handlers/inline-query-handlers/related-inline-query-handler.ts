import { InlineQuery, InlineQueryResultArticle } from '@lightweight-clients/telegram-bot-api-lightweight-client';
import { InlineQueryHandler } from '../query-handler';
import { KnownInlineAnswers } from '../../constants/known-inline-answers';
import { getRelated, toAbsoluteUrl } from '../../../../api-clients/shikimori/shikimori-client';
import { InfoCommandDto, RelatedCommandDto } from '../../command-dtos';

const canHandle = (inlineQuery: InlineQuery): boolean =>
    inlineQuery.query?.startsWith(RelatedCommandDto.Command) ?? false;

const handler: InlineQueryHandler = async (inlineQuery) => {
    console.log('Handling dubs inline query {Query}', inlineQuery.query);

    const commandDto = RelatedCommandDto.fromPayload(inlineQuery.query) as RelatedCommandDto;
    if (!commandDto) {
        console.warn('Failed to deserialize command', inlineQuery.query);
        return [];
    }

    const related = await getRelated(commandDto.myAnimeListId);
    const relatedAnimes = related.filter(x => !!x.anime);
    const items: InlineQueryResultArticle[] = relatedAnimes.length === 0
        ? [
            {
                type: 'article',
                id: KnownInlineAnswers.NoRelatedAnime,
                title: KnownInlineAnswers.NoRelatedAnime,
                input_message_content: {
                    message_text: KnownInlineAnswers.NoRelatedAnime,
                },
            },
        ]
        : relatedAnimes.map(item => ({
            type: 'article',
            id: item.anime!.id.toString(),
            title: item.anime!.russian || item.anime!.name,
            description: [item.relation_russian, item.anime!.aired_on?.substring(0, 4)].filter(x => !!x).join(', '),
            thumbnail_url: item.anime!.image.preview ? toAbsoluteUrl(item.anime!.image.preview) : undefined,
            input_message_content: {
                message_text: new InfoCommandDto(item.anime!.id).toString(),
            },
        }));

    console.log('Returning {Count} related animes for {MyAnimeListId}', items.length, commandDto.myAnimeListId);
    return items;
}

export const relatedInlineQueryHandler = {
    canHandle,
    handler,
};
