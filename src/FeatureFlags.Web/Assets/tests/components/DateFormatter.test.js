/**
 * Unit tests for nilla-date.
 * Doesn't try to test 3rd party fecha library.
 */

import {
    describe, expect, it,
} from 'vitest';
import { format } from 'fecha';
import { isRendered } from '../utils';
import '../../js/components/DateFormatter';

const dateString = '2024-02-27 01:23:45';
const testDate = new Date(dateString);
const localeDateString = testDate.toLocaleString();
const invalidDateString = 'gibberish';
const dateFormat = 'YYYY-MM-DD hh:mm:ss.SSS A';
const formattedDateString = format(testDate, dateFormat);

/**
 * Gets the date formatter element.
 * @returns {HTMLElement | null | undefined} Date formatter element
 */
function getDateFormatter() {
    return document.body.querySelector('nilla-date');
}

describe('date formatter with no format', async () => {
    it('should have locale formatted date when date is valid', async () => {
        document.body.innerHTML = `<nilla-date>${dateString}</nilla-date>`;
        await isRendered(getDateFormatter);

        const dateFormatter = getDateFormatter();

        expect(dateFormatter).toBeTruthy();
        expect(dateFormatter.textContent).toEqual(localeDateString);
    });

    it('should have unchanged content when date is omitted', async () => {
        document.body.innerHTML = '<nilla-date></nilla-date>';
        await isRendered(getDateFormatter);

        const dateFormatter = getDateFormatter();

        expect(dateFormatter).toBeTruthy();
        expect(dateFormatter.textContent).toEqual('');
    });

    it('should have unchanged content when date is invalid', async () => {
        document.body.innerHTML = `<nilla-date>${invalidDateString}</nilla-date>`;
        await isRendered(getDateFormatter);

        const dateFormatter = getDateFormatter();

        expect(dateFormatter).toBeTruthy();
        expect(dateFormatter.textContent).toEqual(invalidDateString);
    });
});

describe('date formatter with format', async () => {
    it('should have correctly formatted date when date is valid', async () => {
        document.body.innerHTML = `<nilla-date data-date-format="${dateFormat}">${dateString}</nilla-date>`;
        await isRendered(getDateFormatter);

        const dateFormatter = getDateFormatter();

        expect(dateFormatter).toBeTruthy();
        expect(dateFormatter.textContent).toEqual(formattedDateString);
    });

    it('should have unchanged content when date is omitted', async () => {
        document.body.innerHTML = `<nilla-date data-date-format="${dateFormat}"></nilla-date>`;
        await isRendered(getDateFormatter);

        const dateFormatter = getDateFormatter();

        expect(dateFormatter).toBeTruthy();
        expect(dateFormatter.textContent).toEqual('');
    });

    it('should have unchanged content when date is invalid', async () => {
        document.body.innerHTML = `<nilla-date data-date-format="${dateFormat}">${invalidDateString}</nilla-date>`;
        await isRendered(getDateFormatter);

        const dateFormatter = getDateFormatter();

        expect(dateFormatter).toBeTruthy();
        expect(dateFormatter.textContent).toEqual(invalidDateString);
    });
});
