/**
 * Unit tests for nilla-copy-to-clipboard.
 */

import { describe, it, beforeEach } from 'node:test';
import assert from 'node:assert/strict';
import tick from '../testUtils/tick.js';
import setupDom from '../testUtils/setupDom.js';
import isRendered from '../testUtils/isRendered.js';

// Setup jsdom first
await setupDom();

// Import the custom element after jsdom is set up
await import('../../js/components/CopyToClipboard.js');

/**
 * Gets the nilla-copy-to-clipboard element.
 * @returns {HTMLElement | null} Copy to clipboard element
 */
function getCopyElement() {
    return document.querySelector('nilla-copy-to-clipboard');
}

/**
 * Gets the copy button inside nilla-copy-to-clipboard.
 * @returns {HTMLElement | null} Copy button element
 */
function getCopyButton() {
    return getCopyElement()?.querySelector('[data-copy-button]');
}

/**
 * Gets the first data-copy-content element inside nilla-copy-to-clipboard.
 * @returns {HTMLElement | null} Content element to copy
 */
function getCopyContent() {
    return getCopyElement()?.querySelector('[data-copy-content]');
}

describe('CopyToClipboard', () => {
    beforeEach(async () => {
        document.body.innerHTML = `
            <nilla-copy-to-clipboard>
                <input data-copy-content value="copied value" />
                <button data-copy-button>Copy</button>
            </nilla-copy-to-clipboard>
        `;
        await isRendered(getCopyElement);
    });

    it('should copy input value to clipboard when button is clicked', async () => {
        let calledWith;
        global.navigator.clipboard = {
            writeText: (text) => {
                calledWith = text;
            },
        };

        const button = getCopyButton();
        button.click();
        await tick();

        assert.strictEqual(calledWith, 'copied value', 'Should copy input value to clipboard');
    });

    it('should copy textContent if value is not present', async () => {
        const button = getCopyButton();
        const input = getCopyContent();
        input.value = '';
        input.textContent = 'fallback text';
        let calledWith;
        global.navigator.clipboard = {
            writeText: (text) => {
                calledWith = text;
            },
        };

        button.click();
        await tick();

        assert.strictEqual(calledWith, 'fallback text', 'Should copy textContent to clipboard if value is empty');
    });

    it('should not throw if clipboard API is unavailable', async () => {
        const button = getCopyButton();
        global.navigator.clipboard = undefined;
        assert.doesNotThrow(() => button.click(), 'Should not throw if clipboard API is unavailable');
    });

    it('should do nothing if there is no [data-copy-content] element', async () => {
        document.body.innerHTML = `<nilla-copy-to-clipboard><button data-copy-button>Copy</button></nilla-copy-to-clipboard>`;
        await isRendered(getCopyElement);

        const button = getCopyButton();
        let called = false;
        global.navigator.clipboard = {
            writeText: () => {
                called = true;
            },
        };

        button.click();
        await tick();

        assert.strictEqual(called, false, 'Should not call writeText if no [data-copy-content] element');
    });

    it('should do nothing if there is no value or textContent to copy', async () => {
        document.body.innerHTML = `
            <nilla-copy-to-clipboard><input data-copy-content value="" /><button data-copy-button>Copy</button></nilla-copy-to-clipboard>
        `;
        await isRendered(getCopyElement);

        const button = getCopyButton();
        const input = getCopyContent();
        input.value = '';
        input.textContent = '';
        let called = false;
        global.navigator.clipboard = {
            writeText: () => {
                called = true;
            },
        };

        button.click();
        await tick();

        assert.strictEqual(called, false, 'Should not call writeText if nothing to copy');
    });

    it('should copy text from a div with data-copy-content', async () => {
        document.body.innerHTML = `
            <nilla-copy-to-clipboard><div data-copy-content>div text to copy</div><button data-copy-button>Copy</button></nilla-copy-to-clipboard>
        `;
        await isRendered(getCopyElement);

        const button = getCopyButton();
        let calledWith;
        global.navigator.clipboard = {
            writeText: (text) => {
                calledWith = text;
            },
        };

        button.click();
        await tick();

        assert.strictEqual(calledWith, 'div text to copy', 'Should copy div textContent to clipboard');
    });

    it('should copy text from a span with data-copy-content', async () => {
        document.body.innerHTML = `
            <nilla-copy-to-clipboard><span data-copy-content>span text to copy</span><button data-copy-button>Copy</button></nilla-copy-to-clipboard>
        `;
        await isRendered(getCopyElement);

        const button = getCopyButton();
        let calledWith;
        global.navigator.clipboard = {
            writeText: (text) => {
                calledWith = text;
            },
        };

        button.click();
        await tick();

        assert.strictEqual(calledWith, 'span text to copy', 'Should copy span textContent to clipboard');
    });

    it('should copy text from the first data-copy-content element if multiple exist', async () => {
        document.body.innerHTML = `
            <nilla-copy-to-clipboard>
                <input data-copy-content value="first value" />
                <div data-copy-content>second value</div>
                <button data-copy-button>Copy</button>
            </nilla-copy-to-clipboard>
        `;
        await isRendered(getCopyElement);

        const button = getCopyButton();
        let calledWith;
        global.navigator.clipboard = {
            writeText: (text) => {
                calledWith = text;
            },
        };

        button.click();
        await tick();

        assert.strictEqual(calledWith, 'first value', 'Should copy value from the first data-copy-content element');
    });
});
