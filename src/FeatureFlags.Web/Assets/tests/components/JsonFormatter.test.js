/**
 * Unit tests for nilla-json.
 * Doesn't try to test 3rd party prettyPrintJson library.
 */

import assert from 'node:assert/strict';
import { beforeEach, describe, it } from 'node:test';
import isRendered from '../testUtils/isRendered.js';
import setupDom from '../testUtils/setupDom.js';

// Setup jsdom first
await setupDom();

// Import the custom element after jsdom is set up
await import('../../js/components/JsonFormatter.js');

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

describe('json formatter with content', () => {
    beforeEach(async () => {
        document.body.innerHTML = jsonFormatterHtml;
        await isRendered(getJsonFormatter);
    });

    it('should have test json', async () => {
        const jsonFormatter = getJsonFormatter();
        const pre = getPre();

        assert.ok(jsonFormatter, 'Json formatter element should exist');
        assert.ok(pre, 'Pre element should exist');
        assert.ok(pre.innerHTML.includes(testString), 'Pre should contain test string');
    });

    it('should have pre json container', async () => {
        const jsonFormatter = getJsonFormatter();
        const pre = getPre();

        assert.ok(jsonFormatter, 'Json formatter element should exist');
        assert.ok(pre, 'Pre element should exist');
        assert.ok(pre.classList.contains('json-container'), 'Pre should have json-container class');
    });
});

describe('json formatter with no content', () => {
    beforeEach(async () => {
        document.body.innerHTML = emptyJsonFormatterHtml;
        await isRendered(getJsonFormatter);
    });

    it('should have empty json', async () => {
        const jsonFormatter = getJsonFormatter();
        const pre = getPre();

        assert.ok(jsonFormatter, 'Json formatter element should exist');
        assert.ok(pre, 'Pre element should exist');
        assert.ok(pre.innerHTML.includes('{}'), 'Pre should contain {} for empty json');
    });

    it('should have pre json container', async () => {
        const jsonFormatter = getJsonFormatter();
        const pre = getPre();

        assert.ok(jsonFormatter, 'Json formatter element should exist');
        assert.ok(pre, 'Pre element should exist');
        assert.ok(pre.classList.contains('json-container'), 'Pre should have json-container class');
    });
});

describe('json formatter with invalid content', () => {
    beforeEach(async () => {
        document.body.innerHTML = invalidJsonFormatterHtml;
        await isRendered(getJsonFormatter);
    });

    it('should have invalid json', async () => {
        const jsonFormatter = getJsonFormatter();

        assert.ok(jsonFormatter, 'Json formatter element should exist');
        assert.ok(jsonFormatter.innerHTML.includes(invalidJson), 'Json formatter should contain invalid json');
    });

    it('should not have pre json container', async () => {
        const jsonFormatter = getJsonFormatter();
        const pre = getPre();

        assert.ok(jsonFormatter, 'Json formatter element should exist');
        assert.ok(!pre, 'Pre element should not exist for invalid json');
    });
});
