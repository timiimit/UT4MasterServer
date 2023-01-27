import { Md5 } from "ts-md5";

export function isWellDefined(value: unknown) {
    return value !== undefined && value !== null;
}

export function objectHash(obj: unknown) {
    if (!isWellDefined(obj)) {
        return '';
    }
    return Md5.hashStr(JSON.stringify(obj));
}