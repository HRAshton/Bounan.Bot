/* eslint-disable @typescript-eslint/no-explicit-any */

import { camelCase } from 'change-case';

type PascalToCamelCase<S extends string> = S extends `${infer F}${infer R}`
    ? `${Lowercase<F>}${R}`
    : S;

export type KeysToCamelCase<T> = {
    [K in keyof T as PascalToCamelCase<string & K>]: KeysToCamelCase<T[K]>
};

export const toCamelCase = <T>(obj: T): KeysToCamelCase<T> => {
    if (Array.isArray(obj)) {
        return obj.map((v) => toCamelCase(v)) as any;
    } else if (!!obj && (obj as any).constructor === Object) {
        return Object.keys(obj as any).reduce(
            (result, key) => ({
                ...result,
                [camelCase(key)]: toCamelCase((obj as any)[key] as any),
            }),
            {},
        ) as any;
    } else {
        return obj as any;
    }
}