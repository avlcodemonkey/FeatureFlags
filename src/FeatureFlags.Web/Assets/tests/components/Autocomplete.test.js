/**
 * Unit tests for nilla-autocomplete.
 * Doesn't try to test 3rd party autocompleter library.
 */

import {
    beforeEach, describe, expect, it,
} from 'vitest';
import { isRendered } from '../utils';
import '../../js/components/Autocomplete';

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
    return getAutocomplete().querySelector('[data-autocomplete-value]');
}
/**
 * Gets the display input element.
 * @returns {HTMLElement | null | undefined} Display input element
 */
function getDisplayInput() {
    return getAutocomplete().querySelector('[data-autocomplete-display]');
}

describe('autocomplete with valid markup', async () => {
    beforeEach(async () => {
        document.body.innerHTML = autocompleteHtml;
        await isRendered(getAutocomplete);
    });

    it('should have custom attributes added', async () => {
        const autocomplete = getAutocomplete();
        const displayInput = getDisplayInput();
        const valueInput = getValueInput();
        // autocomplete library adds this attribute so its a good indicator that autocompleter initialized
        const popupAttr = displayInput.attributes['aria-haspopup'];

        expect(autocomplete).toBeTruthy();
        expect(displayInput).toBeTruthy();
        expect(valueInput).toBeTruthy();
        expect(popupAttr).toBeTruthy();
        expect(popupAttr.value).toEqual('listbox');
    });
});

describe('autocomplete with no src attribute', async () => {
    beforeEach(async () => {
        document.body.innerHTML = noSrcAutocompleteHtml;
        await isRendered(getAutocomplete);
    });

    it('should not have custom attributes added', async () => {
        const autocomplete = getAutocomplete();
        const displayInput = getDisplayInput();
        const valueInput = getValueInput();
        // autocomplete library adds this attribute so its a good indicator that autocompleter initialized
        const popupAttr = displayInput.attributes['aria-haspopup'];

        expect(autocomplete).toBeTruthy();
        expect(displayInput).toBeTruthy();
        expect(valueInput).toBeTruthy();
        expect(popupAttr).toBeFalsy();
    });
});

describe('autocomplete with no value input', async () => {
    beforeEach(async () => {
        document.body.innerHTML = noValueInputAutocompleteHtml;
        await isRendered(getAutocomplete);
    });

    it('should not have custom attributes added', async () => {
        const autocomplete = getAutocomplete();
        const displayInput = getDisplayInput();
        const valueInput = getValueInput();
        // autocomplete library adds this attribute so its a good indicator that autocompleter initialized
        const popupAttr = displayInput.attributes['aria-haspopup'];

        expect(autocomplete).toBeTruthy();
        expect(displayInput).toBeTruthy();
        expect(valueInput).toBeFalsy();
        expect(popupAttr).toBeFalsy();
    });
});
