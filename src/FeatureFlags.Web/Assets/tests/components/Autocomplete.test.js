/**
 * Unit tests for nilla-autocomplete.
 * Doesn't try to test 3rd party autocompleter library.
 */

import assert from 'node:assert/strict';
import { beforeEach, describe, it } from 'node:test';
import isRendered from '../testUtils/isRendered.js';
import setupDom from '../testUtils/setupDom.js';

// Setup jsdom first
await setupDom();

// Import the custom element after jsdom is set up
await import('../../js/components/Autocomplete.js');

const autocompleteHtml = `
    <nilla-autocomplete data-empty-message="No matching results." data-src-url="/AuditLog/UserList">
        <input data-autocomplete-value type="hidden" value="" />
        <input autocomplete="off" data-autocomplete-display type="text" />
    </nilla-autocomplete>
`;

const noSrcAutocompleteHtml = `
    <nilla-autocomplete data-empty-message="No matching results.">
        <input data-autocomplete-value type="hidden" value="" />
        <input autocomplete="off" data-autocomplete-display type="text" />
    </nilla-autocomplete>
`;

const noValueInputAutocompleteHtml = `
    <nilla-autocomplete data-empty-message="No matching results." data-src-url="/AuditLog/UserList">
        <input autocomplete="off" data-autocomplete-display type="text" />
    </nilla-autocomplete>
`;

/**
 * Gets the autocomplete element.
 * @returns {HTMLElement | null | undefined} Autocomplete element
 */
function getAutocomplete() {
    return document.body.querySelector('nilla-autocomplete');
}

/**
 * Gets the value input element.
 * @returns {HTMLElement | null | undefined} Value input element
 */
function getValueInput() {
    return getAutocomplete()?.querySelector('[data-autocomplete-value]');
}

/**
 * Gets the display input element.
 * @returns {HTMLElement | null | undefined} Display input element
 */
function getDisplayInput() {
    return getAutocomplete()?.querySelector('[data-autocomplete-display]');
}

describe('autocomplete with valid markup', () => {
    beforeEach(async () => {
        document.body.innerHTML = autocompleteHtml;
        await isRendered(getAutocomplete);
    });

    it('should have custom attributes added', async () => {
        const autocomplete = getAutocomplete();
        const displayInput = getDisplayInput();
        const valueInput = getValueInput();
        const popupAttr = displayInput.attributes['aria-haspopup'];

        assert.ok(autocomplete, 'Autocomplete element should exist');
        assert.ok(displayInput, 'Display input should exist');
        assert.ok(valueInput, 'Value input should exist');
        assert.ok(popupAttr, 'aria-haspopup attribute should exist');
        assert.strictEqual(popupAttr.value, 'listbox', 'aria-haspopup value should be listbox');
    });

    it('should clear value input when display input changes', async () => {
        const displayInput = getDisplayInput();
        const valueInput = getValueInput();

        // Simulate a selection
        displayInput.value = 'Test Label';
        displayInput.dataset.label = 'Test Label';
        valueInput.value = 'Test Value';

        // Change display input to something else
        displayInput.value = 'Changed';
        displayInput.dispatchEvent(new window.Event('change'));

        assert.strictEqual(displayInput.value, '', 'Display input should be cleared');
        assert.strictEqual(valueInput.value, '', 'Value input should be cleared');
    });

    it('should destroy autocompleter on disconnectedCallback', async () => {
        const autocomplete = getAutocomplete();
        let destroyCalled = false;
        // Patch destroy method
        autocomplete.autocompleter.destroy = () => {
            destroyCalled = true;
        };

        // Remove from DOM
        autocomplete.remove();
        // disconnectedCallback is called automatically by jsdom
        assert.ok(destroyCalled, 'destroy should be called on disconnectedCallback');
    });
});

describe('autocomplete with no src attribute', () => {
    beforeEach(async () => {
        document.body.innerHTML = noSrcAutocompleteHtml;
        await isRendered(getAutocomplete);
    });

    it('should not have custom attributes added', async () => {
        const autocomplete = getAutocomplete();
        const displayInput = getDisplayInput();
        const valueInput = getValueInput();
        const popupAttr = displayInput.attributes['aria-haspopup'];

        assert.ok(autocomplete, 'Autocomplete element should exist');
        assert.ok(displayInput, 'Display input should exist');
        assert.ok(valueInput, 'Value input should exist');
        assert.ok(!popupAttr, 'aria-haspopup attribute should not exist');
    });
});

describe('autocomplete with no value input', () => {
    beforeEach(async () => {
        document.body.innerHTML = noValueInputAutocompleteHtml;
        await isRendered(getAutocomplete);
    });

    it('should not have custom attributes added', async () => {
        const autocomplete = getAutocomplete();
        const displayInput = getDisplayInput();
        const valueInput = getValueInput();
        const popupAttr = displayInput.attributes['aria-haspopup'];

        assert.ok(autocomplete, 'Autocomplete element should exist');
        assert.ok(displayInput, 'Display input should exist');
        assert.ok(!valueInput, 'Value input should not exist');
        assert.ok(!popupAttr, 'aria-haspopup attribute should not exist');
    });
});
