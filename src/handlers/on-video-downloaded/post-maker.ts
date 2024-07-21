// import { Scenes, VideoKey } from '../../shared/models';
// import { ShikiAnimeInfo } from '../../api-clients/shikimori/shiki-anime-info';
// import { CopyMessageData } from 'telegram-bot-api-lightweight-client/src/types';
// import { Texts } from '../../shared/telegram/texts';
//
// export type MessageToSend = Pick<CopyMessageData, 'message_id' | 'caption' | 'reply_markup'>;
//
// export const createMessageForOneTimeNotification = (
//     animeInfo: ShikiAnimeInfo,
//     videoKey: VideoKey,
//     videoMessageId: number,
//     scenes: Scenes | undefined,
// ): MessageToSend => {
//     if (!videoMessageId) {
//         throw new Error('Message id is not provided');
//     }
//
//     const sharedText = createSharedText(animeInfo, videoKey, scenes);
//
//     return {
//         caption: sharedText,
//         message_id: videoMessageId,
//         reply_markup: {
//             inline_keyboard: [
//                 [
//                     {
//                         text: Texts.Button__Unsubscribe,
//                         callback_data: JSON.stringify({
//                             action: ,
//                             videoKey,
//                         }),
//                     },
//                     {
//                         text: '🔗 MAL',
//                         url: `https://myanimelist.net/anime/${animeInfo.id}`,
//                     },
//                 ],
//             ],
//         },
//     };
// }