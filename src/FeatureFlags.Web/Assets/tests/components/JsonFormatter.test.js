/**
 * Unit tests for nilla-json.
 * Doesn't try to test 3rd party prettyPrintJson library.
 */

import {
    beforeEach, describe, expect, it,
} from 'vitest';
import { isRendered } from '../utils';
import '../../js/components/JsonFormatter';

const testString = 'find this test string';
const testObj = {
    a: testString, c: 'd', e: 123, f: new Date(),
};
const testJson = JSON.stringify(testObj);

const invalidJson = '{...}';

const jsonFormatterHtml = `
    <nilla-json>${testJson}</nilla-json>
`;

const emptyJsonFormatterHtml = `
    <nilla-json></nilla-json>
`;

const invalidJsonFormatterHtml = `
    <nilla-json>${invalidJson}</nilla-json>
`;

/**
 * Gets the json formatter element.
 * @returns {HTMLElement | null | undefined} Json formatter element
 */
function getJsonFormatter() {
    return document.body.querySelector('nilla-json');
}

/**
 * Gets the pre that contains the json content.
 * @returns {HTMLElement | null | undefined} Pre
 */
function getPre() {
    return getJsonFormatter()?.querySelector('pre');
}

describe('json formatter with content', async () => {
    beforeEach(async () => {
        document.body.innerHTML = jsonFormatterHtml;
        await isRendered(getJsonFormatter);
    });

    it('should have test json', async () => {
        const jsonFormatter = getJsonFormatter();
        const pre = getPre();

        expect(jsonFormatter).toBeTruthy();
        expect(pre).toBeTruthy();
        expect(pre.innerHTML).toContain(testString);
    });

    it('should have pre json container', async () => {
        const jsonFormatter = getJsonFormatter();
        const pre = getPre();

        expect(jsonFormatter).toBeTruthy();
        expect(pre).toBeTruthy();
        expect(pre.classList).toContain('json-container');
    });
});

describe('json formatter with no content', async () => {
    beforeEach(async () => {
        document.body.innerHTML = emptyJsonFormatterHtml;
        await isRendered(getJsonFormatter);
    });

    it('should have empty json', async () => {
        const jsonFormatter = getJsonFormatter();
        const pre = getPre();

        expect(jsonFormatter).toBeTruthy();
        expect(pre).toBeTruthy();
        expect(pre.innerHTML).toContain('{}');
    });

    it('should have pre json container', async () => {
        const jsonFormatter = getJsonFormatter();
        const pre = getPre();

        expect(jsonFormatter).toBeTruthy();
        expect(pre).toBeTruthy();
        expect(pre.classList).toContain('json-container');
    });
});

describe('json formatter with invalid content', async () => {
    beforeEach(async () => {
        document.body.innerHTML = invalidJsonFormatterHtml;
        await isRendered(getJsonFormatter);
    });

    it('should have invalid json', async () => {
        const jsonFormatter = getJsonFormatter();

        expect(jsonFormatter).toBeTruthy();
        expect(jsonFormatter.innerHTML).toContain(invalidJson);
    });

    it('should not have pre json container', async () => {
        const jsonFormatter = getJsonFormatter();
        const pre = getPre();

        expect(jsonFormatter).toBeTruthy();
        expect(pre).toBeFalsy();
    });
});
