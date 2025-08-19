import { Message } from '@lightweight-clients/telegram-bot-api-lightweight-client';
import {
    copyMessage,
    CopyMessageData,
    InlineKeyboardMarkup,
    sendMessage,
} from '@lightweight-clients/telegram-bot-api-lightweight-client';

import { getVideoInfo } from '../../../../api-clients/animan/animan-client';
import { getAllExistingVideos } from '../../../../api-clients/cached-loan-api-client';
import { getShikiAnimeInfo } from '../../../../api-clients/cached-shikimori-client';
import { config } from '../../../../config/config';
import { assert } from '../../../../shared/helpers/assert';
import { dubToKey } from '../../../../shared/helpers/dub-to-key';
import { BotResponse, VideoKey } from '../../../../shared/models';
import { getKeyboard } from '../../../../shared/telegram/get-keyboard';
import { getVideoDescription } from '../../../../shared/telegram/get-video-description';
import { Texts } from '../../../../shared/telegram/texts';
import { subscribeOneTime } from '../../../subscriptions-repository';
import { WatchCommandDto } from '../../command-dtos';
import { MessageHandler } from '../query-handler';

const sendSwitchDubButtons = async (chatId: number, searchResults: VideoKey[], episode: number) => {
    const inOtherDubs = searchResults
        .filter(item => item.episode === episode)
        .sort((a, b) => a.dub.localeCompare(b.dub));

    console.log('Episode not found in dub. Other dubs: ', inOtherDubs.map(ep => ep.dub));

    await sendMessage({
        chat_id: chatId,
        text: Texts.Message__EpisodeWithDubNotFound,
        reply_markup: {
            inline_keyboard: inOtherDubs.map(ep => [{
                text: ep.dub,
                callback_data: new WatchCommandDto(ep.myAnimeListId, dubToKey(ep.dub), episode).toString(),
            }]),
        },
    });
}

const sendVideo = async (
    message: Pick<Message, 'chat' | 'text'>,
    commandDto: WatchCommandDto,
    videoInfo: BotResponse,
    keyboard: InlineKeyboardMarkup,
) => {
    const animeInfo = await getShikiAnimeInfo(commandDto.myAnimeListId);
    const videoDescription = getVideoDescription(animeInfo, commandDto, videoInfo.scenes);

    const args: CopyMessageData = {
        chat_id: message.chat.id,
        from_chat_id: config.value.telegram.videoChatId,
        message_id: videoInfo.messageId!,
        caption: videoDescription,
        reply_markup: keyboard,
        parse_mode: 'HTML',
    };

    console.log('Sending video: ', JSON.stringify(args));
    await copyMessage(args);
}

const sendVideoResult = async (
    message: Pick<Message, 'chat' | 'text'>,
    commandDto: WatchCommandDto,
    searchResults: VideoKey[],
    selectedVideo: VideoKey,
) => {
    const episodesInDub = searchResults
        .filter(item => dubToKey(item.dub) === commandDto.dub)
        .map(item => item.episode)
        .sort((a, b) => a - b);
    if (episodesInDub.length === 0) {
        throw new Error('No episodes found');
    }

    const videoInfo = await getVideoInfo(selectedVideo);
    console.log('Anime info: ', JSON.stringify(videoInfo));

    const keyboard = getKeyboard(commandDto, episodesInDub, videoInfo?.publishingDetails);

    switch (videoInfo?.status) {
        case 'Pending':
        case 'Downloading':
            console.log('Video not downloaded');
            await Promise.all([
                await subscribeOneTime(selectedVideo, message.chat.id),
                await sendMessage({
                    chat_id: message.chat.id,
                    text: Texts.Message__VideoIsCooking,
                    reply_markup: keyboard,
                }),
            ]);
            break;

        case 'Failed':
        case 'NotAvailable':
            console.log('Video failed to download');
            await sendMessage({
                chat_id: message.chat.id,
                text: Texts.ErrorOnEpisode,
                reply_markup: keyboard,
            });
            break;

        case 'Downloaded':
            console.log('Sending video');
            await sendVideo(message, commandDto, videoInfo, keyboard);
            break;

        case null:
            console.error('Lambda returned null');
            await sendMessage({
                chat_id: message.chat.id,
                text: Texts.UnknownError,
                reply_markup: keyboard,
            });
            break;

        default:
            throw new Error(`Unknown video status: ${videoInfo.status}`);
    }
}

const canHandle = (message: Message): boolean => message.text?.startsWith(WatchCommandDto.Command) ?? false;

const handler: MessageHandler = async (message) => {
    assert(!!message.text);
    assert(!!message.chat?.id);

    console.log('Received watch command');

    const commandDto = WatchCommandDto.fromPayload(message.text!) as WatchCommandDto;
    console.log('Parsed command: ', commandDto);
    if (!commandDto) {
        console.warn('Failed to deserialize command', message.text);
        return;
    }

    const searchResults = await getAllExistingVideos(commandDto.myAnimeListId);
    if (!searchResults || searchResults.length === 0) {
        console.log('No videos found');
        await sendMessage({
            chat_id: message.chat.id,
            text: Texts.Search__NoResultsInLoan,
        });
        return;
    }

    const selectedVideo = searchResults
        .find(v => dubToKey(v.dub) === commandDto.dub && v.episode === commandDto.episode);
    console.log('Selected video: ', selectedVideo);
    if (!selectedVideo) {
        console.warn('Video not found in dub');
        await sendSwitchDubButtons(message.chat.id, searchResults, commandDto.episode);
        return;
    }

    await sendVideoResult(message, commandDto, searchResults, selectedVideo);

    console.log('Watch command handled');
}

export const watchMessageHandler = {
    canHandle,
    handler,
};