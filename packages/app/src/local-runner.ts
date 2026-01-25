import type { VideoRegisteredNotification } from '../../../third-party/common/ts/interfaces';
import { handler as onSchedule } from './handlers/on-schedule/handler';

const onRegistered = async (message: VideoRegisteredNotification) => {
  console.log('Processing message: ', message);

  // @ts-expect-error - we don't need to provide all the event properties
  await videoRegistered({ Records: [{ Sns: { Message: JSON.stringify(message) } }] });

  console.log('Message processed');
}

const main = async () => {
  const animes: [number, string][] = [
    [59730, 'РуАниме / DEEP'],
    [801, 'MC Entertainment'],
  ]

  console.log('TEST: Episodes should be registered on the first run');
  await onRegistered({
    items: [
      {
        videoKey: {
          myAnimeListId: animes[0][0],
          dub: animes[0][1],
          episode: 1,
        },
      },
      {
        videoKey: {
          myAnimeListId: animes[0][0],
          dub: animes[0][1],
          episode: 2,
        },
      },
      {
        videoKey: {
          myAnimeListId: animes[0][0],
          dub: animes[0][1],
          episode: 3,
        },
      },
    ],
  });
  console.warn('Expected: 3 episodes registered\n\n');

  console.log('TEST: Episodes should not be registered twice');
  await onRegistered({
    items: [
      {
        videoKey: {
          myAnimeListId: animes[0][0],
          dub: animes[0][1],
          episode: 2,
        },
      },
    ],
  });
  console.warn('Expected: No episodes registered\n\n');

  console.log('TEST: Only new episodes should be registered');
  await onRegistered({
    items: [
      {
        videoKey: {
          myAnimeListId: animes[0][0],
          dub: animes[0][1],
          episode: 2,
        },
      },
      {
        videoKey: {
          myAnimeListId: animes[0][0],
          dub: animes[0][1],
          episode: 3,
        },
      },
      {
        videoKey: {
          myAnimeListId: animes[0][0],
          dub: animes[0][1],
          episode: 4,
        },
      },
      {
        videoKey: {
          myAnimeListId: animes[0][0],
          dub: animes[0][1],
          episode: 5,
        },
      },
    ],
  });
  console.warn('Expected: 4&5 episodes registered\n\n');

  console.log('TEST: Different titles should be registered separately');
  await onRegistered({
    items: [
      {
        videoKey: {
          myAnimeListId: animes[1][0],
          dub: animes[1][1],
          episode: 1,
        },
      },
      {
        videoKey: {
          myAnimeListId: animes[0][0],
          dub: animes[0][1],
          episode: 2,
        },
      },
      {
        videoKey: {
          myAnimeListId: animes[1][0],
          dub: animes[1][1],
          episode: 3,
        },
      },
    ],
  });
  console.warn('Expected: 2 episodes registered for 1st title, 1 episode for 2nd title\n\n');

  console.log('TEST: When we add last episode, the anime should be deleted');
  await onRegistered({
    items: Array.from({ length: 24 }, (_, i) => ({
      videoKey: {
        myAnimeListId: animes[0][0],
        dub: animes[0][1],
        episode: i + 1,
      },
    })),
  });
  console.warn('Expected: Anime deleted\n\n');

  console.log('TEST: On Schedule');
  await onSchedule({} as never);
  console.warn('Expected: No errors\n\n');
}

main();
