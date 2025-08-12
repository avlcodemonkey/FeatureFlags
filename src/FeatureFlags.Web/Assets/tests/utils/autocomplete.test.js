/**
 * Unit tests for autocomplete functionality.
 */

import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest';
import autocomplete from '../../js/utils/autocomplete';

describe('autocomplete', () => {
    let input;
    let container;
    let cleanupFns = [];

    beforeEach(() => {
        // Set up DOM elements
        input = document.createElement('input');
        document.body.appendChild(input);
        container = document.createElement('div');
        document.body.appendChild(container);
    });

    afterEach(() => {
        // Clean up DOM
        input.remove();
        container.remove();
        cleanupFns.forEach(fn => fn());
        cleanupFns = [];
    });

    it('throws if input is not provided', () => {
        expect(() => autocomplete({})).toThrow('input undefined');
    });

    it('attaches autocomplete container to DOM on fetch', async () => {
        const fetchMock = vi.fn((text, cb) => cb([{ label: 'foo' }]));
        const ac = autocomplete({
            input,
            fetch: fetchMock,
        });
        input.value = 'fo';
        input.dispatchEvent(new Event('input'));
        await new Promise(r => setTimeout(r, 10));
        const acContainer = document.querySelector('.autocomplete');
        expect(acContainer).not.toBeNull();
        ac.destroy();
    });

    it('renders suggestions and calls onSelect on click', async () => {
        const fetchMock = vi.fn((text, cb) => cb([{ label: 'bar' }]));
        const onSelect = vi.fn();
        const ac = autocomplete({
            input,
            fetch: fetchMock,
            onSelect,
        });
        input.value = 'bar';
        input.dispatchEvent(new Event('input'));
        await new Promise(r => setTimeout(r, 10));
        const suggestion = document.querySelector('.autocomplete div');
        expect(suggestion).not.toBeNull();
        suggestion.dispatchEvent(new MouseEvent('click', { bubbles: true }));
        expect(onSelect).toHaveBeenCalledWith({ label: 'bar' }, input);
        ac.destroy();
    });

    it('shows empty message if no suggestions and emptyMsg is set', async () => {
        const fetchMock = vi.fn((text, cb) => cb([]));
        const ac = autocomplete({
            input,
            fetch: fetchMock,
            emptyMsg: 'No results',
        });
        input.value = 'zzz';
        input.dispatchEvent(new Event('input'));
        await new Promise(r => setTimeout(r, 10));
        const empty = document.querySelector('.autocomplete .empty');
        expect(empty).not.toBeNull();
        expect(empty.textContent).toBe('No results');
        ac.destroy();
    });

    it('destroys and cleans up event listeners', () => {
        const fetchMock = vi.fn((text, cb) => cb([]));
        const ac = autocomplete({
            input,
            fetch: fetchMock,
        });
        ac.destroy();
        // After destroy, input events should not trigger fetch
        input.value = 'test';
        input.dispatchEvent(new Event('input'));
        expect(fetchMock).not.toHaveBeenCalled();
    });

    it('does not show suggestions if input is below minLength', async () => {
        const fetchMock = vi.fn((text, cb) => cb([{ label: 'foo' }]));
        const ac = autocomplete({
            input,
            fetch: fetchMock,
            minLength: 3,
        });
        input.value = 'fo';
        input.dispatchEvent(new Event('input'));
        await new Promise(r => setTimeout(r, 10));
        const acContainer = document.querySelector('.autocomplete');
        expect(acContainer).toBeNull();
        ac.destroy();
    });

    it('shows suggestions on focus if showOnFocus is true', async () => {
        const fetchMock = vi.fn((text, cb) => cb([{ label: 'focus' }]));
        const ac = autocomplete({
            input,
            fetch: fetchMock,
            showOnFocus: true,
        });
        input.value = '';
        input.dispatchEvent(new Event('focus'));
        await new Promise(r => setTimeout(r, 10));
        const suggestion = document.querySelector('.autocomplete div');
        expect(suggestion).not.toBeNull();
        ac.destroy();
    });

    it('does not auto-select first suggestion if disableAutoSelect is true', async () => {
        const fetchMock = vi.fn((text, cb) => cb([{ label: 'one' }, { label: 'two' }]));
        const ac = autocomplete({
            input,
            fetch: fetchMock,
            disableAutoSelect: true,
        });
        input.value = 'o';
        input.dispatchEvent(new Event('input'));
        await new Promise(r => setTimeout(r, 10));
        const selected = document.querySelector('.autocomplete .selected');
        expect(selected).toBeNull();
        ac.destroy();
    });

    it('calls custom render function if provided', async () => {
        const fetchMock = vi.fn((text, cb) => cb([{ label: 'custom' }]));
        const render = vi.fn((item) => {
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
        input.dispatchEvent(new Event('input'));
        await new Promise(r => setTimeout(r, 10));
        expect(render).toHaveBeenCalled();
        const suggestion = document.querySelector('.autocomplete div');
        expect(suggestion.textContent).toBe('Custom: custom');
        ac.destroy();
    });

    it('debounces fetch calls according to debounceWaitMs', async () => {
        vi.useFakeTimers();
        const fetchMock = vi.fn((text, cb) => cb([{ label: text }]));
        const ac = autocomplete({
            input,
            fetch: fetchMock,
            debounceWaitMs: 100,
        });
        input.value = 'a';
        input.dispatchEvent(new Event('input'));
        input.value = 'ab';
        input.dispatchEvent(new Event('input'));
        vi.advanceTimersByTime(99);
        expect(fetchMock).not.toHaveBeenCalled();
        vi.advanceTimersByTime(1);
        expect(fetchMock).toHaveBeenCalled();
        vi.useRealTimers();
        ac.destroy();
    });

    it('selects suggestion with keyboard and calls onSelect on Enter', async () => {
        const fetchMock = vi.fn((text, cb) => cb([{ label: 'alpha' }, { label: 'beta' }]));
        const onSelect = vi.fn();
        const ac = autocomplete({
            input,
            fetch: fetchMock,
            onSelect,
        });
        input.value = 'ch';
        input.dispatchEvent(new Event('input'));
        await new Promise(r => setTimeout(r, 10));
        // Simulate ArrowDown to select second suggestion
        input.dispatchEvent(new KeyboardEvent('keydown', { key: 'ArrowDown' }));
        // Simulate Enter to select
        input.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter' }));
        expect(onSelect).toHaveBeenCalledWith({ label: 'beta' }, input);
        ac.destroy();
    });

    it('clears suggestions on Escape key', async () => {
        const fetchMock = vi.fn((text, cb) => cb([{ label: 'esc' }]));
        const ac = autocomplete({
            input,
            fetch: fetchMock,
        });
        input.value = 'esc';
        input.dispatchEvent(new Event('input'));
        await new Promise(r => setTimeout(r, 10));
        input.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));
        const acContainer = document.querySelector('.autocomplete');
        expect(acContainer?.parentNode).toBeUndefined();
        ac.destroy();
    });

    it('manual fetch method triggers suggestions', async () => {
        const fetchMock = vi.fn((text, cb) => cb([{ label: 'manual' }]));
        const ac = autocomplete({
            input,
            fetch: fetchMock,
        });
        input.value = 'manual';
        ac.fetch();
        await new Promise(r => setTimeout(r, 10));
        const suggestion = document.querySelector('.autocomplete div');
        expect(suggestion).not.toBeNull();
        ac.destroy();
    });

    it('wraps selection to last suggestion when ArrowUp is pressed at the top', async () => {
        const fetchMock = vi.fn((text, cb) => cb([{ label: 'one' }, { label: 'two' }, { label: 'three' }]));
        const onSelect = vi.fn();
        const ac = autocomplete({
            input,
            fetch: fetchMock,
            onSelect,
        });
        input.value = 'on';
        input.dispatchEvent(new Event('input'));
        await new Promise(r => setTimeout(r, 10));
        // ArrowUp at initial state should select last suggestion
        input.dispatchEvent(new KeyboardEvent('keydown', { key: 'ArrowUp' }));
        input.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter' }));
        expect(onSelect).toHaveBeenCalledWith({ label: 'three' }, input);
        ac.destroy();
    });

    it('wraps selection to first suggestion when ArrowDown is pressed at the bottom', async () => {
        const fetchMock = vi.fn((text, cb) => cb([{ label: 'one' }, { label: 'two' }]));
        const onSelect = vi.fn();
        const ac = autocomplete({
            input,
            fetch: fetchMock,
            onSelect,
        });
        input.value = 'on';
        input.dispatchEvent(new Event('input'));
        await new Promise(r => setTimeout(r, 10));
        // ArrowDown twice: first selects 'two'
        input.dispatchEvent(new KeyboardEvent('keydown', { key: 'ArrowDown' }));
        // ArrowDown again should wrap to first suggestion
        input.dispatchEvent(new KeyboardEvent('keydown', { key: 'ArrowDown' }));
        input.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter' }));
        expect(onSelect).toHaveBeenCalledWith({ label: 'one' }, input);
        ac.destroy();
    });

    it('does nothing on ArrowDown or Enter when there are no suggestions', async () => {
        const fetchMock = vi.fn((text, cb) => cb([]));
        const onSelect = vi.fn();
        const ac = autocomplete({
            input,
            fetch: fetchMock,
            onSelect,
        });
        input.value = 'zz';
        input.dispatchEvent(new Event('input'));
        await new Promise(r => setTimeout(r, 10));
        input.dispatchEvent(new KeyboardEvent('keydown', { key: 'ArrowDown' }));
        input.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter' }));
        expect(onSelect).not.toHaveBeenCalled();
        ac.destroy();
    });

    it('clears suggestions when input is cleared after suggestions are shown', async () => {
        const fetchMock = vi.fn((text, cb) => cb([{ label: 'foo' }]));
        const ac = autocomplete({
            input,
            fetch: fetchMock,
        });
        input.value = 'fo';
        input.dispatchEvent(new Event('input'));
        await new Promise(r => setTimeout(r, 10));
        let acContainer = document.querySelector('.autocomplete');
        expect(acContainer).not.toBeNull();
        // Clear input
        input.value = '';
        input.dispatchEvent(new Event('input'));
        await new Promise(r => setTimeout(r, 10));
        acContainer = document.querySelector('.autocomplete');
        // Should be removed from DOM
        expect(acContainer?.parentNode).toBeUndefined();
        ac.destroy();
    });
});
