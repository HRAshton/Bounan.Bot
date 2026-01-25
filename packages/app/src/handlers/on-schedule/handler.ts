import type { EventBridgeEvent } from 'aws-lambda';

import { initConfig } from '../../config/config';

const process = async (): Promise<void> => {
  console.log('Processing videos');
  // TODO
}

export const handler = async (event: EventBridgeEvent<never, never>): Promise<void> => {
  console.log('Processing event: ', JSON.stringify(event));
  await initConfig();
  await process();
  console.info('done');
};
