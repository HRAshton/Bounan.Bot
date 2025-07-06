import { InlineQueryHandler } from '../query-handler';
import { searchAnime, toAbsoluteUrl } from '../../../../api-clients/shikimori/shikimori-client';
import { getDubs } from '../../../../api-clients/loan-api/src/animan-loan-api-client';
import { InlineQueryResultArticle } from '@lightweight-clients/telegram-bot-api-lightweight-client';
import { InfoCommandDto } from '../../command-dtos';
import { KnownInlineAnswers } from '../../constants/known-inline-answers';

export const handler: InlineQueryHandler = async (inlineQuery) => {
    console.log('Handling search inline query {Query}', inlineQuery.query);

    if (!inlineQuery.query) {
        return [];
    }

    const shikimoriResults = await searchAnime(inlineQuery.query);
    console.log('Got search results for {Query}: {Count}', inlineQuery.query, shikimoriResults.length);

    const loanApiDubs = await Promise.all(shikimoriResults.map(async anime => ({
        anime,
        dubs: await getDubs(anime.id),
    })));
    const availableDubs = loanApiDubs.filter(x => x.dubs.length > 0);

    const results: InlineQueryResultArticle[] = availableDubs.length === 0
        ? [
            {
                type: 'article',
                id: KnownInlineAnswers.NoResults,
                title: KnownInlineAnswers.NoResults,
                input_message_content: {
                    message_text: KnownInlineAnswers.NoResults,
                },
            },
        ]
        : availableDubs.map(pair => ({
            type: 'article',
            id: pair.anime.id.toString(),
            title: pair.anime.russian || pair.anime.name,
            description: [
                pair.anime.aired_on?.substring(0, 4),
                pair.anime.genres?.map(x => x.russian).join(', '),
                pair.dubs.map(x => x.name).sort().join(', '),
            ].filter(x => !!x).join('\n'),
            thumbnail_url: pair.anime!.image.preview ? toAbsoluteUrl(pair.anime!.image.preview) : undefined,
            input_message_content: {
                message_text: new InfoCommandDto(pair.anime.id).toString(),
            },
        }));

    return results;
}

export const searchInlineQueryHandler = {
    handler,
};