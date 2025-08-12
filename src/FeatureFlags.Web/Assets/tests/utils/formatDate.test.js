/**
 * Unit tests for formatDate function.
 */

import {
    describe, expect, it,
} from 'vitest';
import { formatDate } from '../../js/utils/formatDate';

describe('formatDate', () => {
    it('formats a date using the default mask', () => {
        const date = new Date('2023-04-05T14:23:45');
        // default: 'ddd MMM DD YYYY HH:mm:ss'
        expect(formatDate(date)).toBe('Wed Apr 05 2023 14:23:45');
    });

    it('formats a date using shortDate mask', () => {
        const date = new Date('2023-04-05T14:23:45');
        expect(formatDate(date, 'shortDate')).toBe('4/5/23');
    });

    it('formats a date using mediumDate mask', () => {
        const date = new Date('2023-04-05T14:23:45');
        expect(formatDate(date, 'mediumDate')).toBe('Apr 5, 2023');
    });

    it('formats a date using longDate mask', () => {
        const date = new Date('2023-04-05T14:23:45');
        expect(formatDate(date, 'longDate')).toBe('April 5, 2023');
    });

    it('formats a date using fullDate mask', () => {
        const date = new Date('2023-04-05T14:23:45');
        expect(formatDate(date, 'fullDate')).toBe('Wednesday, April 5, 2023');
    });

    it('formats a date using isoDate mask', () => {
        const date = new Date('2023-04-05T14:23:45');
        expect(formatDate(date, 'isoDate')).toBe('2023-04-05');
    });

    it('formats a date using isoDateTime mask', () => {
        const date = new Date(Date.UTC(2023, 3, 5, 14, 23, 45));
        // The output depends on the system timezone, so we generate the expected value dynamically.
        const pad = n => n.toString().padStart(2, '0');
        const local = new Date(date);
        const year = local.getFullYear();
        const month = pad(local.getMonth() + 1);
        const day = pad(local.getDate());
        const hours = pad(local.getHours());
        const minutes = pad(local.getMinutes());
        const seconds = pad(local.getSeconds());
        const offset = -local.getTimezoneOffset();
        const sign = offset >= 0 ? '+' : '-';
        const absOffset = Math.abs(offset);
        const offsetHours = pad(Math.floor(absOffset / 60));
        const offsetMinutes = pad(absOffset % 60);
        const tz = `${sign}${offsetHours}:${offsetMinutes}`;
        const expected = `${year}-${month}-${day}T${hours}:${minutes}:${seconds}${tz}`;
        expect(formatDate(date, 'isoDateTime')).toBe(expected);
    });

    it('formats a date using shortTime mask', () => {
        const date = new Date('2023-04-05T04:05:09');
        expect(formatDate(date, 'shortTime')).toBe('04:05');
    });

    it('formats a date using mediumTime mask', () => {
        const date = new Date('2023-04-05T04:05:09');
        expect(formatDate(date, 'mediumTime')).toBe('04:05:09');
    });

    it('formats a date using longTime mask', () => {
        const date = new Date('2023-04-05T04:05:09.123');
        expect(formatDate(date, 'longTime')).toBe('04:05:09.123');
    });

    it('formats a timestamp', () => {
        const timestamp = Date.UTC(2023, 3, 5, 14, 23, 45);
        expect(formatDate(timestamp, 'isoDate')).toBe('2023-04-05');
    });

    it('throws on invalid date', () => {
        expect(() => formatDate('not a date')).toThrow('Invalid Date pass to format');
    });

    it('formats ordinal days correctly', () => {
        const date = new Date('2023-04-01T00:00:00');
        expect(formatDate(date, 'Do')).toBe('1st');
        expect(formatDate(new Date('2023-04-02T00:00:00'), 'Do')).toBe('2nd');
        expect(formatDate(new Date('2023-04-03T00:00:00'), 'Do')).toBe('3rd');
        expect(formatDate(new Date('2023-04-04T00:00:00'), 'Do')).toBe('4th');
        expect(formatDate(new Date('2023-04-11T00:00:00'), 'Do')).toBe('11th');
        expect(formatDate(new Date('2023-04-21T00:00:00'), 'Do')).toBe('21st');
        expect(formatDate(new Date('2023-04-22T00:00:00'), 'Do')).toBe('22nd');
        expect(formatDate(new Date('2023-04-23T00:00:00'), 'Do')).toBe('23rd');
        expect(formatDate(new Date('2023-04-24T00:00:00'), 'Do')).toBe('24th');
    });

    it('formats a date using a custom mask "YYYY-MM-DD hh:mm:ss A"', () => {
        const date = new Date('2023-04-05T14:23:45');
        // Build expected string using local time
        const pad = n => n.toString().padStart(2, '0');
        const year = date.getFullYear();
        const month = pad(date.getMonth() + 1);
        const day = pad(date.getDate());
        let hours = date.getHours() % 12 || 12;
        const ampm = date.getHours() < 12 ? 'AM' : 'PM';
        hours = pad(hours);
        const minutes = pad(date.getMinutes());
        const seconds = pad(date.getSeconds());
        const expected = `${year}-${month}-${day} ${hours}:${minutes}:${seconds} ${ampm}`;
        expect(formatDate(date, 'YYYY-MM-DD hh:mm:ss A')).toBe(expected);
    });

    it('formats a morning time using custom mask "YYYY-MM-DD hh:mm:ss A"', () => {
        const date = new Date('2023-04-05T04:05:09');
        const pad = n => n.toString().padStart(2, '0');
        const year = date.getFullYear();
        const month = pad(date.getMonth() + 1);
        const day = pad(date.getDate());
        let hours = date.getHours() % 12 || 12;
        const ampm = date.getHours() < 12 ? 'AM' : 'PM';
        hours = pad(hours);
        const minutes = pad(date.getMinutes());
        const seconds = pad(date.getSeconds());
        const expected = `${year}-${month}-${day} ${hours}:${minutes}:${seconds} ${ampm}`;
        expect(formatDate(date, 'YYYY-MM-DD hh:mm:ss A')).toBe(expected);
    });
});
