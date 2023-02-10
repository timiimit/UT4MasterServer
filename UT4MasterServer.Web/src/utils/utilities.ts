import { Md5 } from 'ts-md5';
import { RouteParams } from 'vue-router';

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
  if (!rawtime) {
    return '00:00';
  }
  const hours = Math.floor(rawtime / 3600);
  const minutes = Math.floor((rawtime - hours * 3600) / 60);
  const seconds = rawtime - hours * 3600 - minutes * 60;
  let minutesString = `${minutes}`;
  let secondsString = `${seconds}`;
  if (minutes < 10) {
    minutesString = '0' + minutes;
  }
  if (seconds < 10) {
    secondsString = '0' + seconds;
  }
  return `${minutesString}:${secondsString}`;
}

export function toHoursMinutesSeconds(rawtime?: number) {
  if (!rawtime) {
    return '00:00:00';
  }
  const hours = Math.floor(rawtime / 3600);
  let hoursString = `${hours}`;
  if (hours < 10) {
    hoursString = '0' + hours;
  }
  return `${hoursString}:${toMinutesSeconds(rawtime)}`;
}

type DateTimeStyle = 'short' | 'full' | 'long' | 'medium' | undefined;
export function isoDateTimeStringToLocalDateTime(
  dateTime: string,
  dateStyle: DateTimeStyle = 'short',
  timeStyle: DateTimeStyle = 'short'
) {
  return new Intl.DateTimeFormat('default', { dateStyle, timeStyle }).format(
    new Date(dateTime)
  );
}

export function getRouteParamStringValue(
  params: RouteParams,
  key: string,
  defaultValue: string | undefined = undefined
) {
  const paramString = params[key];
  return paramString?.length ? (paramString as string) : defaultValue;
}

export function getRouteParamNumberValue(
  params: RouteParams,
  key: string,
  defaultValue: number
) {
  const paramString = params[key];
  return paramString?.length ? +paramString : defaultValue;
}
