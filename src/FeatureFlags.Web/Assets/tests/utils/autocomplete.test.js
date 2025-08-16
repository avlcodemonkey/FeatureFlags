/**
 * Unit tests for autocomplete functionality.
 */

import assert from 'node:assert/strict';
import { afterEach, beforeEach, describe, it, mock } from 'node:test';
import autocomplete from '../../js/utils/autocomplete.js';
import setupDom from '../testUtils/setupDom.js';
import tick from '../testUtils/tick.js';

// Setup jsdom first
await setupDom();

describe('autocomplete', () => {
    let input;
    let container;

    beforeEach(() => {
        input = document.createElement('input');
        document.body.appendChild(input);
        container = document.createElement('div');
        document.body.appendChild(container);
    });

    afterEach(() => {
        input.remove();
        container.remove();
        // Remove autocomplete container if present
        const acContainer = document.querySelector('.autocomplete');
        if (acContainer) acContainer.remove();
    });

    it('throws if input is not provided', () => {
        assert.throws(() => autocomplete({}), /input undefined/);
    });

    it('attaches autocomplete container to DOM on fetch', async () => {
        const fetchMock = mock.fn((text, cb) => cb([{ label: 'foo' }]));
        const ac = autocomplete({
            input,
            fetch: fetchMock,
        });
        input.value = 'fo';
        input.dispatchEvent(new global.window.Event('input'));
        await tick();
        const acContainer = document.querySelector('.autocomplete');
        assert.ok(acContainer);
        ac.destroy();
    });

    it('renders suggestions and calls onSelect on click', async () => {
        const fetchMock = mock.fn((text, cb) => cb([{ label: 'bar' }]));
        const onSelect = mock.fn();
        const ac = autocomplete({
            input,
            fetch: fetchMock,
            onSelect,
        });
        input.value = 'bar';
        input.dispatchEvent(new global.window.Event('input'));
        await tick();
        const suggestion = document.querySelector('.autocomplete div');
        assert.ok(suggestion);
        suggestion.dispatchEvent(new global.window.MouseEvent('click', { bubbles: true }));
        assert.deepStrictEqual(onSelect.mock.calls[0].arguments, [{ label: 'bar' }, input]);
        ac.destroy();
    });

    it('shows empty message if no suggestions and emptyMsg is set', async () => {
        const fetchMock = mock.fn((text, cb) => cb([]));
        const ac = autocomplete({
            input,
            fetch: fetchMock,
            emptyMsg: 'No results',
        });
        input.value = 'zzz';
        input.dispatchEvent(new global.window.Event('input'));
        await tick();
        const empty = document.querySelector('.autocomplete .empty');
        assert.ok(empty);
        assert.strictEqual(empty.textContent, 'No results');
        ac.destroy();
    });

    it('destroys and cleans up event listeners', () => {
        const fetchMock = mock.fn((text, cb) => cb([]));
        const ac = autocomplete({
            input,
            fetch: fetchMock,
        });
        ac.destroy();
        input.value = 'test';
        input.dispatchEvent(new global.window.Event('input'));
        assert.strictEqual(fetchMock.mock.callCount(), 0);
    });

    it('does not show suggestions if input is below minLength', async () => {
        const fetchMock = mock.fn((text, cb) => cb([{ label: 'foo' }]));
        const ac = autocomplete({
            input,
            fetch: fetchMock,
            minLength: 3,
        });
        input.value = 'fo';
        input.dispatchEvent(new global.window.Event('input'));
        await tick();
        const acContainer = document.querySelector('.autocomplete');
        assert.strictEqual(acContainer, null);
        ac.destroy();
    });

    it('shows suggestions on focus if showOnFocus is true', async () => {
        const fetchMock = mock.fn((text, cb) => cb([{ label: 'focus' }]));
        const ac = autocomplete({
            input,
            fetch: fetchMock,
            showOnFocus: true,
        });
        input.value = '';
        input.dispatchEvent(new global.window.Event('focus'));
        await tick();
        const suggestion = document.querySelector('.autocomplete div');
        assert.ok(suggestion);
        ac.destroy();
    });

    it('does not auto-select first suggestion if disableAutoSelect is true', async () => {
        const fetchMock = mock.fn((text, cb) => cb([{ label: 'one' }, { label: 'two' }]));
        const ac = autocomplete({
            input,
            fetch: fetchMock,
            disableAutoSelect: true,
        });
        input.value = 'o';
        input.dispatchEvent(new global.window.Event('input'));
        await tick();
        const selected = document.querySelector('.autocomplete .selected');
        assert.strictEqual(selected, null);
        ac.destroy();
    });

    it('calls custom render function if provided', async () => {
        const fetchMock = mock.fn((text, cb) => cb([{ label: 'custom' }]));
        const render = mock.fn((item) => {
            const el = document.createElement('div');
            el.textContent = `Custom: ${item.label}`;
            return el;
        });
        const ac = autocomplete({
            input,
            fetch: fetchMock,
            render,
        });
        input.value = 'ch';
        input.dispatchEvent(new global.window.Event('input'));
        await tick();
        assert.strictEqual(render.mock.callCount(), 1);
        const suggestion = document.querySelector('.autocomplete div');
        assert.strictEqual(suggestion.textContent, 'Custom: custom');
        ac.destroy();
    });

    it('debounces fetch calls according to debounceWaitMs', async () => {
        const fetchMock = mock.fn((text, cb) => cb([{ label: text }]));
        const ac = autocomplete({
            input,
            fetch: fetchMock,
            debounceWaitMs: 100,
        });
        input.value = 'a';
        input.dispatchEvent(new global.window.Event('input'));
        input.value = 'ab';
        input.dispatchEvent(new global.window.Event('input'));
        await new Promise(r => setTimeout(r, 90));
        assert.strictEqual(fetchMock.mock.callCount(), 0);
        await new Promise(r => setTimeout(r, 20));
        assert.strictEqual(fetchMock.mock.callCount(), 1);
        ac.destroy();
    });

    it('selects suggestion with keyboard and calls onSelect on Enter', async () => {
        const fetchMock = mock.fn((text, cb) => cb([{ label: 'alpha' }, { label: 'beta' }]));
        const onSelect = mock.fn();
        const ac = autocomplete({
            input,
            fetch: fetchMock,
            onSelect,
        });
        input.value = 'ch';
        input.dispatchEvent(new global.window.Event('input'));
        await tick();
        input.dispatchEvent(new global.window.KeyboardEvent('keydown', { key: 'ArrowDown' }));
        input.dispatchEvent(new global.window.KeyboardEvent('keydown', { key: 'Enter' }));
        assert.deepStrictEqual(onSelect.mock.calls[0].arguments, [{ label: 'beta' }, input]);
        ac.destroy();
    });

    it('clears suggestions on Escape key', async () => {
        const fetchMock = mock.fn((text, cb) => cb([{ label: 'esc' }]));
        const ac = autocomplete({
            input,
            fetch: fetchMock,
        });
        input.value = 'esc';
        input.dispatchEvent(new global.window.Event('input'));
        await tick();
        input.dispatchEvent(new global.window.KeyboardEvent('keydown', { key: 'Escape' }));
        const acContainer = document.querySelector('.autocomplete');
        assert.ok(!acContainer?.parentNode);
        ac.destroy();
    });

    it('manual fetch method triggers suggestions', async () => {
        const fetchMock = mock.fn((text, cb) => cb([{ label: 'manual' }]));
        const ac = autocomplete({
            input,
            fetch: fetchMock,
        });
        input.value = 'manual';
        ac.fetch();
        await tick();
        const suggestion = document.querySelector('.autocomplete div');
        assert.ok(suggestion);
        ac.destroy();
    });

    it('wraps selection to last suggestion when ArrowUp is pressed at the top', async () => {
        const fetchMock = mock.fn((text, cb) => cb([{ label: 'one' }, { label: 'two' }, { label: 'three' }]));
        const onSelect = mock.fn();
        const ac = autocomplete({
            input,
            fetch: fetchMock,
            onSelect,
        });
        input.value = 'on';
        input.dispatchEvent(new global.window.Event('input'));
        await tick();
        input.dispatchEvent(new global.window.KeyboardEvent('keydown', { key: 'ArrowUp' }));
        input.dispatchEvent(new global.window.KeyboardEvent('keydown', { key: 'Enter' }));
        assert.deepStrictEqual(onSelect.mock.calls[0].arguments, [{ label: 'three' }, input]);
        ac.destroy();
    });

    it('wraps selection to first suggestion when ArrowDown is pressed at the bottom', async () => {
        const fetchMock = mock.fn((text, cb) => cb([{ label: 'one' }, { label: 'two' }]));
        const onSelect = mock.fn();
        const ac = autocomplete({
            input,
            fetch: fetchMock,
            onSelect,
        });
        input.value = 'on';
        input.dispatchEvent(new global.window.Event('input'));
        await tick();
        input.dispatchEvent(new global.window.KeyboardEvent('keydown', { key: 'ArrowDown' }));
        input.dispatchEvent(new global.window.KeyboardEvent('keydown', { key: 'ArrowDown' }));
        input.dispatchEvent(new global.window.KeyboardEvent('keydown', { key: 'Enter' }));
        assert.deepStrictEqual(onSelect.mock.calls[0].arguments, [{ label: 'one' }, input]);
        ac.destroy();
    });

    it('does nothing on ArrowDown or Enter when there are no suggestions', async () => {
        const fetchMock = mock.fn((text, cb) => cb([]));
        const onSelect = mock.fn();
        const ac = autocomplete({
            input,
            fetch: fetchMock,
            onSelect,
        });
        input.value = 'zz';
        input.dispatchEvent(new global.window.Event('input'));
        await tick();
        input.dispatchEvent(new global.window.KeyboardEvent('keydown', { key: 'ArrowDown' }));
        input.dispatchEvent(new global.window.KeyboardEvent('keydown', { key: 'Enter' }));
        assert.strictEqual(onSelect.mock.callCount(), 0);
        ac.destroy();
    });

    it('clears suggestions when input is cleared after suggestions are shown', async () => {
        const fetchMock = mock.fn((text, cb) => cb([{ label: 'foo' }]));
        const ac = autocomplete({
            input,
            fetch: fetchMock,
        });
        input.value = 'fo';
        input.dispatchEvent(new global.window.Event('input'));
        await tick();
        let acContainer = document.querySelector('.autocomplete');
        assert.ok(acContainer);
        input.value = '';
        input.dispatchEvent(new global.window.Event('input'));
        await tick();
        acContainer = document.querySelector('.autocomplete');
        assert.ok(!acContainer?.parentNode);
        ac.destroy();
    });

    it('removes all event listeners after destroy', async () => {
        const fetchMock = mock.fn((text, cb) => cb([{ label: 'foo' }]));
        const ac = autocomplete({ input, fetch: fetchMock });
        input.value = 'foo';
        input.dispatchEvent(new global.window.Event('input'));
        await tick();
        ac.destroy();

        // Try to trigger input event after destroy
        input.value = 'bar';
        input.dispatchEvent(new global.window.Event('input'));
        await tick();
        // fetchMock should not be called again
        assert.strictEqual(fetchMock.mock.callCount(), 1);
    });

    it('handles slow fetches and only shows latest results', async () => {
        const fetchMock = mock.fn((text, cb) => {
            if (text === 'a') {
                setTimeout(() => cb([{ label: 'A' }]), 100);
            }
            if (text === 'b') {
                setTimeout(() => cb([{ label: 'B' }]), 50);
            }
        });
        const ac = autocomplete({
            input, fetch: fetchMock,
            debounceWaitMs: 1, minLength: 1,
        });

        input.value = 'a';
        input.dispatchEvent(new global.window.Event('input'));
        input.value = 'b';
        input.dispatchEvent(new global.window.Event('input'));
        await new Promise(r => setTimeout(r, 200));

        const suggestion = document.querySelector('.autocomplete div');
        assert.ok(suggestion);
        assert.strictEqual(suggestion.textContent, 'B');
        ac.destroy();
    });

    it('handles failed fetches gracefully', async () => {
        // eslint-disable-next-line no-unused-vars
        const fetchMock = mock.fn((text, cb) => {
            throw new Error('Network error');
        });
        const ac = autocomplete({ input, fetch: fetchMock });
        input.value = 'fail';
        assert.doesNotThrow(() => {
            input.dispatchEvent(new global.window.Event('input'));
        });
        await tick();
        // Should not render suggestions
        const acContainer = document.querySelector('.autocomplete');
        assert.ok(!acContainer?.childElementCount);
        ac.destroy();
    });

    it('supports multiple instances independently', async () => {
        const input2 = document.createElement('input');
        document.body.appendChild(input2);

        const fetchMock1 = mock.fn((text, cb) => cb([{ label: 'one' }]));
        const fetchMock2 = mock.fn((text, cb) => cb([{ label: 'two' }]));

        const ac1 = autocomplete({ input, fetch: fetchMock1 });
        const ac2 = autocomplete({ input: input2, fetch: fetchMock2 });

        input.value = 'one';
        input.dispatchEvent(new global.window.Event('input'));
        input2.value = 'two';
        input2.dispatchEvent(new global.window.Event('input'));
        await tick();

        const suggestion1 = document.querySelector('.autocomplete div');
        assert.ok(suggestion1);
        assert.strictEqual(suggestion1.textContent, 'one');

        // Find the second autocomplete container
        const containers = document.querySelectorAll('.autocomplete');
        assert.strictEqual(containers.length, 2);

        // Clean up
        ac1.destroy();
        ac2.destroy();
        input2.remove();
    });
});
