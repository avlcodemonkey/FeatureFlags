/**
 * Unit tests for nilla-date.
 */

import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import { formatDate } from '../../js/utils/formatDate.js';
import isRendered from '../testUtils/isRendered.js';
import setupDom from '../testUtils/setupDom.js';

// Setup jsdom first
await setupDom();

// Import the custom element after jsdom is set up
await import('../../js/components/DateFormatter.js');

const dateString = '2024-02-27 01:23:45';
const testDate = new Date(`${dateString}Z`);
const invalidDateString = 'gibberish';
const dateFormat = 'YYYY-MM-DD hh:mm:ss.SSS A';
const formattedDateString = formatDate(testDate, dateFormat);

/**
 * Gets the date formatter element.
 * @returns {HTMLElement | null | undefined} Date formatter element
 */
function getDateFormatter() {
    return document.body.querySelector('nilla-date');
}

describe('date formatter with no format', () => {
    it('should show date in locale format when format string is empty', async () => {
        const expected = new Date(`${dateString}Z`).toLocaleString();

        document.body.innerHTML = `<nilla-date data-date-format="">${dateString}</nilla-date>`;
        await isRendered(getDateFormatter);

        const dateFormatter = getDateFormatter();
        assert.ok(dateFormatter, 'Date formatter element should exist');
        assert.strictEqual(dateFormatter.textContent, expected, 'Should show date in locale format for empty format');
    });

    it('should have unchanged content when date is omitted', async () => {
        document.body.innerHTML = '<nilla-date></nilla-date>';
        await isRendered(getDateFormatter);

        const dateFormatter = getDateFormatter();

        assert.ok(dateFormatter, 'Date formatter element should exist');
        assert.strictEqual(dateFormatter.textContent, '', 'Should show empty string when date is omitted');
    });

    it('should have unchanged content when date is invalid', async () => {
        document.body.innerHTML = `<nilla-date>${invalidDateString}</nilla-date>`;
        await isRendered(getDateFormatter);

        const dateFormatter = getDateFormatter();

        assert.ok(dateFormatter, 'Date formatter element should exist');
        assert.strictEqual(dateFormatter.textContent, invalidDateString, 'Should show original content when date is invalid');
    });
});

describe('date formatter with format', () => {
    it('should have correctly formatted date when date is valid', async () => {
        document.body.innerHTML = `<nilla-date data-date-format="${dateFormat}">${dateString}</nilla-date>`;
        await isRendered(getDateFormatter);

        const dateFormatter = getDateFormatter();

        assert.ok(dateFormatter, 'Date formatter element should exist');
        assert.strictEqual(dateFormatter.textContent, formattedDateString, 'Should show formatted date string');
    });

    it('should have unchanged content when date is omitted', async () => {
        document.body.innerHTML = `<nilla-date data-date-format="${dateFormat}"></nilla-date>`;
        await isRendered(getDateFormatter);

        const dateFormatter = getDateFormatter();

        assert.ok(dateFormatter, 'Date formatter element should exist');
        assert.strictEqual(dateFormatter.textContent, '', 'Should show empty string when date is omitted');
    });

    it('should have unchanged content when date is invalid', async () => {
        document.body.innerHTML = `<nilla-date data-date-format="${dateFormat}">${invalidDateString}</nilla-date>`;
        await isRendered(getDateFormatter);

        const dateFormatter = getDateFormatter();

        assert.ok(dateFormatter, 'Date formatter element should exist');
        assert.strictEqual(dateFormatter.textContent, invalidDateString, 'Should show original content when date is invalid');
    });

    it('should show format string when format string is unsupported', async () => {
        const unsupportedFormat = 'XXXXXX';
        document.body.innerHTML = `<nilla-date data-date-format="${unsupportedFormat}">${dateString}</nilla-date>`;
        await isRendered(getDateFormatter);

        const dateFormatter = getDateFormatter();
        assert.ok(dateFormatter, 'Date formatter element should exist');
        assert.strictEqual(dateFormatter.textContent, unsupportedFormat, 'Should show format string for unsupported format');
    });
});

describe('date formatter edge cases', () => {
    it('should handle leap year date', async () => {
        const leapDateString = '2024-02-29 12:00:00';
        const leapDate = new Date(`${leapDateString}Z`);
        const expected = leapDate.toLocaleString();

        document.body.innerHTML = `<nilla-date>${leapDateString}</nilla-date>`;
        await isRendered(getDateFormatter);

        const dateFormatter = getDateFormatter();
        assert.ok(dateFormatter, 'Date formatter element should exist');
        assert.strictEqual(dateFormatter.textContent, expected, 'Should show locale formatted leap year date');
    });

    it('should handle very old date', async () => {
        const oldDateString = '1900-01-01 00:00:00';
        const oldDate = new Date(`${oldDateString}Z`);
        const expected = oldDate.toLocaleString();

        document.body.innerHTML = `<nilla-date>${oldDateString}</nilla-date>`;
        await isRendered(getDateFormatter);

        const dateFormatter = getDateFormatter();
        assert.ok(dateFormatter, 'Date formatter element should exist');
        assert.strictEqual(dateFormatter.textContent, expected, 'Should show locale formatted old date');
    });

    it('should handle very future date', async () => {
        const futureDateString = '3000-12-31 23:59:59';
        const futureDate = new Date(`${futureDateString}Z`);
        const expected = futureDate.toLocaleString();

        document.body.innerHTML = `<nilla-date>${futureDateString}</nilla-date>`;
        await isRendered(getDateFormatter);

        const dateFormatter = getDateFormatter();
        assert.ok(dateFormatter, 'Date formatter element should exist');
        assert.strictEqual(dateFormatter.textContent, expected, 'Should show locale formatted future date');
    });

    it('should handle ambiguous date string', async () => {
        const ambiguousDateString = '02/03/2024 01:23:45'; // MM/DD/YYYY or DD/MM/YYYY
        const ambiguousDate = new Date(`${ambiguousDateString}Z`);
        const expected = ambiguousDate.toLocaleString();

        document.body.innerHTML = `<nilla-date>${ambiguousDateString}</nilla-date>`;
        await isRendered(getDateFormatter);

        const dateFormatter = getDateFormatter();
        assert.ok(dateFormatter, 'Date formatter element should exist');
        assert.strictEqual(dateFormatter.textContent, expected, 'Should show locale formatted ambiguous date');
    });

    it('should handle time zone offset', async () => {
        const tzDateString = '2024-02-27T01:23:45+05:00';
        const tzDate = new Date(tzDateString);
        const expected = formatDate(tzDate, dateFormat);

        document.body.innerHTML = `<nilla-date data-date-format="${dateFormat}">${tzDateString}</nilla-date>`;
        await isRendered(getDateFormatter);

        const dateFormatter = getDateFormatter();
        assert.ok(dateFormatter, 'Date formatter element should exist');
        assert.strictEqual(dateFormatter.textContent, expected, 'Should show locale formatted date with time zone');
    });
});
