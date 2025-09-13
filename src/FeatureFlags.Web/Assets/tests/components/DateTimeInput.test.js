/**
 * Unit tests for nilla-datetime-input.
 */

import assert from 'node:assert/strict';
import { beforeEach, describe, it } from 'node:test';
import isRendered from '../testUtils/isRendered.js';
import setupDom from '../testUtils/setupDom.js';
import tick from '../testUtils/tick.js';

// Setup jsdom first
await setupDom();

// Import the custom element after jsdom is set up
await import('../../js/components/DateTimeInput.js');

const html = `
    <div>
        <nilla-datetime-input>
            <input type="datetime-local" data-datetime-local>
            <input type="hidden" value="2024-06-01T12:34:56.000Z" data-datetime-hidden>
        </nilla-datetime-input>
    </div>
`;

/**
 * Gets the component custom element.
 * @returns {HTMLElement | null | undefined} Component element
 */
function getComponent() {
    return document.body.querySelector('nilla-datetime-input');
}

describe('DateTimeInput', () => {
    beforeEach(async () => {
        document.body.innerHTML = html;
        await isRendered(getComponent);
    });

    it('should set local input value from hidden UTC value on load', async () => {
        const container = getComponent();
        const localInputAfter = container.querySelector('[data-datetime-local]');

        assert.ok(localInputAfter.value, 'Local input should be set from hidden UTC value');
        // Should be formatted as 'YYYY-MM-DDTHH:mm:ss.SSS'
        assert.match(localInputAfter.value, /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d{3}$/, 'Local input value should match expected format');
    });

    it('should clear local input if hidden UTC value is invalid', async () => {
        const container = getComponent();
        const hiddenInput = container.querySelector('[data-datetime-hidden]');
        hiddenInput.value = 'not-a-date';
        container.innerHTML = `
            <input type="datetime-local" data-datetime-local>
            <input type="hidden" data-datetime-hidden value="not-a-date">
        `;
        customElements.upgrade(container);

        const localInputAfter = container.querySelector('[data-datetime-local]');
        assert.strictEqual(localInputAfter.value, '', 'Local input should be empty for invalid UTC value');
    });

    it('should set hidden input to UTC when local input changes', async () => {
        const container = getComponent();
        const localInput = container.querySelector('[data-datetime-local]');
        const hiddenInput = container.querySelector('[data-datetime-hidden]');
        localInput.value = '2024-06-01T12:34:56';
        // Trigger change event
        localInput.dispatchEvent(new window.Event('change'));
        await tick();

        assert.ok(hiddenInput.value, 'Hidden input should be set');
        assert.match(hiddenInput.value, /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$/, 'Hidden input should be valid UTC string');
    });

    it('should clear hidden input if local input is invalid', async () => {
        const container = getComponent();
        const localInput = container.querySelector('[data-datetime-local]');
        const hiddenInput = container.querySelector('[data-datetime-hidden]');
        localInput.value = 'not-a-date';
        localInput.dispatchEvent(new window.Event('change'));
        await tick();

        assert.strictEqual(hiddenInput.value, '', 'Hidden input should be empty for invalid local value');
    });

    it('should clear hidden input if local input is empty', async () => {
        const container = getComponent();
        const localInput = container.querySelector('[data-datetime-local]');
        const hiddenInput = container.querySelector('[data-datetime-hidden]');
        localInput.value = '';
        hiddenInput.value = 'should-be-cleared';
        localInput.dispatchEvent(new window.Event('change'));
        await tick();

        assert.strictEqual(hiddenInput.value, '', 'Hidden input should be empty if local input is empty');
    });
});
