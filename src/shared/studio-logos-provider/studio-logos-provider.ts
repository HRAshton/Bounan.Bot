import { config } from '../../config/config';
import { studioLogosList } from './studio-logos-list';

const subsTokens = [
    'subtitles',
    'subs',
    'sub',
    'субтитры',
]

export const getStudioLogoUrl = (studioName: string): string | undefined => {
    const isSubs = subsTokens.some(token => studioName.toLowerCase().includes(token));
    const trimmedStudioName = subsTokens
        .reduce((acc, token) => acc.replace(token, ''), studioName.toLowerCase())
        .replaceAll('/', '')
        .replace(/\.$/, '')
        .replace(/\.tv$/, '')
        .replace(/\.tv old$/, '');
    const expectedImageKey = trimmedStudioName + (isSubs ? '-s' : '');

    let imageKey = studioLogosList.find(item => item.toLowerCase() === expectedImageKey);

    if (!imageKey && isSubs) {
        console.warn('Failed to find logo for {StudioName}. Using default icon for subs', studioName);
        imageKey = '!Subs';
    }

    if (!imageKey) {
        console.warn('Failed to find logo for {StudioName}', studioName);
        return undefined;
    }

    return config.value.assets.studioLogosUrl.replace('{image-key}', encodeURIComponent(imageKey));
}