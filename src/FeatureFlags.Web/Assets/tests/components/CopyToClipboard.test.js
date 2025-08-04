/**
 * Unit tests for nilla-copy-to-clipboard.
 */

import {
    beforeEach, describe, expect, it, vi,
} from 'vitest';
import { tick } from '../utils';
import '../../js/components/CopyToClipboard';

describe('CopyToClipboard', () => {
    let copyEl, button, input;

    beforeEach(() => {
        document.body.innerHTML = `
            <nilla-copy-to-clipboard>
                <input data-copy-content value="copied value" />
                <button data-copy-button>Copy</button>
            </nilla-copy-to-clipboard>
        `;
        copyEl = document.querySelector('nilla-copy-to-clipboard');
        button = copyEl.querySelector('[data-copy-button]');
        input = copyEl.querySelector('[data-copy-content]');
    });

    it('should copy input value to clipboard when button is clicked', async () => {
        const writeText = vi.fn();
        Object.assign(navigator, { clipboard: { writeText } });

        await button.click();
        await tick();

        expect(writeText).toHaveBeenCalledWith('copied value');
    });

    it('should copy textContent if value is not present', async () => {
        input.value = '';
        input.textContent = 'fallback text';
        const writeText = vi.fn();
        Object.assign(navigator, { clipboard: { writeText } });

        await button.click();
        await tick();

        expect(writeText).toHaveBeenCalledWith('fallback text');
    });

    it('should not throw if clipboard API is unavailable', async () => {
        navigator.clipboard = undefined;
        expect(() => button.click()).not.toThrow();
    });

    it('should do nothing if there is no [data-copy-content] element', async () => {
        copyEl.innerHTML = `<button data-copy-button>Copy</button>`;
        const writeText = vi.fn();
        Object.assign(navigator, { clipboard: { writeText } });

        await button.click();
        await tick();

        expect(writeText).not.toHaveBeenCalled();
    });

    it('should do nothing if there is no value or textContent to copy', async () => {
        input.value = '';
        input.textContent = '';
        const writeText = vi.fn();
        Object.assign(navigator, { clipboard: { writeText } });

        await button.click();
        await tick();

        expect(writeText).not.toHaveBeenCalled();
    });
});
