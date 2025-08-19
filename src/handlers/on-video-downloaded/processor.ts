import {
    copyMessage,
    CopyMessageData,
    InlineKeyboardMarkup,
    sendMessage,
    SendMessageData,
} from '@lightweight-clients/telegram-bot-api-lightweight-client';

import { getAllExistingVideos } from '../../api-clients/cached-loan-api-client';
import { getShikiAnimeInfo } from '../../api-clients/cached-shikimori-client';
import { config } from '../../config/config';
import { getKeyboard } from '../../shared/telegram/get-keyboard';
import { getVideoDescription } from '../../shared/telegram/get-video-description';
import { Texts } from '../../shared/telegram/texts';
import { VideoDownloadedNotification } from './models';

const sendVideoMessages = async (
    videoMessageId: number,
    caption: string,
    keyboard: InlineKeyboardMarkup,
    chatIds: number[],
): Promise<void> => {
    for (const chatId of chatIds) {
        const messageToSend: CopyMessageData = {
            chat_id: chatId,
            caption,
            message_id: videoMessageId,
            from_chat_id: config.value.telegram.videoChatId,
            reply_markup: keyboard,
            parse_mode: 'HTML',
        };

        const result = await copyMessage(messageToSend);
        if (!result.ok) {
            console.error('Error copying message: ', JSON.stringify(result));
        }

        await new Promise(resolve => setTimeout(resolve, 100)); // to avoid rate limits
    }
}

const sendErrorMessages = async (caption: string, keyboard: InlineKeyboardMarkup, chatIds: number[]): Promise<void> => {
    console.log('Sending error messages');

    for (const chatId of chatIds) {
        const messageToSend: SendMessageData = {
            chat_id: chatId,
            text: Texts.ErrorOnEpisode + '\n' + caption,
            reply_markup: keyboard,
            parse_mode: 'HTML',
        };

        const result = await sendMessage(messageToSend);
        if (!result.ok) {
            console.error('Error sending error message: ', JSON.stringify(result));
        }

        await new Promise(resolve => setTimeout(resolve, 100)); // to avoid rate limits
    }
}

export const process = async (videoDownloadedNotification: VideoDownloadedNotification): Promise<void> => {
    console.log('Processing videos: ', JSON.stringify(videoDownloadedNotification));

    if (!videoDownloadedNotification.subscriberChatIds?.length) {
        console.log('No subscribers');
        return;
    }

    const animeInfo = await getShikiAnimeInfo(videoDownloadedNotification.videoKey.myAnimeListId);

    const description = getVideoDescription(
        animeInfo,
        videoDownloadedNotification.videoKey,
        videoDownloadedNotification.scenes);
    const episodes = await getAllExistingVideos(parseInt(animeInfo.id));
    const videosWithDub = episodes.filter(x => x.dub === videoDownloadedNotification.videoKey.dub);
    const keyboard = getKeyboard(
        videoDownloadedNotification.videoKey,
        videosWithDub.map(x => x.episode),
        videoDownloadedNotification.publishingDetails,
    );

    if (videoDownloadedNotification.messageId) {
        const { messageId, subscriberChatIds } = videoDownloadedNotification;
        await sendVideoMessages(messageId, description, keyboard, subscriberChatIds as number[]);
    } else {
        await sendErrorMessages(description, keyboard, videoDownloadedNotification.subscriberChatIds as number[]);
    }

    console.log('Animes processed');
}