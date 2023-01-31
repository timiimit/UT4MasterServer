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

export function toMinutesSeconds(rawtime?: number) {
    if(!rawtime) {
        return '00:00';
    }
    var hours = Math.floor(rawtime / 3600);
    var minutes = Math.floor((rawtime - (hours * 3600)) / 60);
    var seconds = rawtime - hours * 3600 - minutes * 60;
    var minutesString = `${minutes}`;
    var secondsString = `${seconds}`;
    if (minutes < 10) {
        minutesString = "0" + minutes;
    }
    if (seconds < 10) {
        secondsString = "0" + seconds;
    }
    return `${minutesString}:${secondsString}`;
}

export function toHoursMinutesSeconds(rawtime?: number) {
    if(!rawtime) {
        return '00:00:00';
    }
    var hours = Math.floor(rawtime / 3600);
    var hoursString = `${hours}`;
    if (hours < 10) {
        hoursString = "0" + hours;
    }
    return `${hoursString}:${toMinutesSeconds(rawtime)}`;
}
