/**
 * Unit tests for nilla-slider-input.
 */

import assert from 'node:assert/strict';
import { beforeEach, describe, it } from 'node:test';
import isRendered from '../testUtils/isRendered.js';
import setupDom from '../testUtils/setupDom.js';
import tick from '../testUtils/tick.js';

// Setup jsdom first
await setupDom();

// Import the custom element after jsdom is set up
await import('../../js/components/SliderInput.js');

const html = `
    <nilla-slider-input>
        <input type="range" min="0" max="100" value="50" data-slider-range>
        <input type="number" min="0" max="100" value="50" data-slider-text>
    </nilla-slider-input>
`;

/**
 * Gets the slider input custom element.
 * @returns {HTMLElement | null | undefined} Slider input element
 */
function getSliderInput() {
    return document.body.querySelector('nilla-slider-input');
}

/**
 * Gets the range input element.
 * @returns {HTMLInputElement | null | undefined} Range input element
 */
function getRangeInput() {
    return getSliderInput()?.querySelector('[data-slider-range]');
}

/**
 * Gets the text/number input element.
 * @returns {HTMLInputElement | null | undefined} Text input element
 */
function getTextInput() {
    return getSliderInput()?.querySelector('[data-slider-text]');
}

describe('SliderInput', () => {
    beforeEach(async () => {
        document.body.innerHTML = html;
        await isRendered(getSliderInput);
    });

    it('should initialize both inputs with the same value', () => {
        const rangeInput = getRangeInput();
        const textInput = getTextInput();
        assert.ok(rangeInput, 'Range input should exist');
        assert.ok(textInput, 'Text input should exist');
        assert.strictEqual(rangeInput.value, '50', 'Range input should be initialized to 50');
        assert.strictEqual(textInput.value, '50', 'Text input should be initialized to 50');
    });

    it('should update text input when range input changes', async () => {
        const rangeInput = getRangeInput();
        const textInput = getTextInput();
        rangeInput.value = '75';
        rangeInput.dispatchEvent(new window.Event('input'));
        await tick();
        assert.strictEqual(textInput.value, '75', 'Text input should update to match range input');
    });

    it('should update range input when text input changes', async () => {
        const rangeInput = getRangeInput();
        const textInput = getTextInput();
        textInput.value = '25';
        textInput.dispatchEvent(new window.Event('input'));
        await tick();
        assert.strictEqual(rangeInput.value, '25', 'Range input should update to match text input');
    });

    it('should handle min/max constraints', async () => {
        const rangeInput = getRangeInput();
        const textInput = getTextInput();
        textInput.value = '150';
        textInput.dispatchEvent(new window.Event('input'));
        await tick();
        // Range input won't accept the value, it'll set to max instead
        assert.strictEqual(rangeInput.value, '100', 'Range input should not reflect out-of-bounds value');
    });

    it('should not throw if inputs are missing', async () => {
        document.body.innerHTML = `<nilla-slider-input></nilla-slider-input>`;
        await isRendered(getSliderInput);
        assert.doesNotThrow(() => {
            getSliderInput().dispatchEvent(new window.Event('input'));
        }, 'Should not throw if inputs are missing');
    });
});
