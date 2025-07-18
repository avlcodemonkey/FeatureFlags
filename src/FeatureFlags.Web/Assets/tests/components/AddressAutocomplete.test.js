/**
 * Unit tests for nilla-address-autocomplete.
 * Doesn't try to test 3rd party autocompleter library.
 */

import {
    beforeEach, describe, expect, it,
} from 'vitest';
import { isRendered } from '../utils';
import '../../js/components/AddressAutocomplete';

// Added all relevant address input fields for more complete testing
const autocompleteHtml = `
    <nilla-address-autocomplete data-empty-message="No matching results." data-src-url="/Location/AddressList">
        <input autocomplete="off" data-address-display type="text" />
        <input autocomplete="off" data-address-address1 type="text" />
        <input autocomplete="off" data-address-address2 type="text" />
        <input autocomplete="off" data-address-city type="text" />
        <input autocomplete="off" data-address-stateCode type="text" />
        <input autocomplete="off" data-address-postalCode type="text" />
        <input autocomplete="off" data-address-countryCode type="text" />
        <input autocomplete="off" data-address-longitude type="text" />
        <input autocomplete="off" data-address-latitude type="text" />
    </nilla-address-autocomplete>
`;

const noSrcAutocompleteHtml = `
    <nilla-address-autocomplete data-empty-message="No matching results.">
        <input autocomplete="off" data-address-display type="text" />
    </nilla-address-autocomplete>
`;

const noDisplayInputAutocompleteHtml = `
    <nilla-address-autocomplete data-empty-message="No matching results." data-src-url="/Location/AddressList">
        <p>loren ipsum dolor sit amet</p>
    </nilla-address-autocomplete>
`;

/**
 * Gets the autocomplete element.
 * @returns {HTMLElement | null | undefined} Autocomplete element
 */
function getAutocomplete() {
    return document.body.querySelector('nilla-address-autocomplete');
}

/**
 * Gets the display input element.
 * @returns {HTMLElement | null | undefined} Display input element
 */
function getDisplayInput() {
    return getAutocomplete().querySelector('[data-address-display]');
}

/**
 * Helper function to get address fields
 * @param {string} attribute Field name to get
 * @returns {HTMLElement | null | undefined} Input element for the specified address field
 */
function getInputByDataAttribute(attribute) {
    return getAutocomplete().querySelector(`[${attribute}]`);
}

describe('autocomplete with valid markup', async () => {
    beforeEach(async () => {
        document.body.innerHTML = autocompleteHtml;
        await isRendered(getAutocomplete);
    });

    it('should have custom attributes added', async () => {
        const autocomplete = getAutocomplete();
        const displayInput = getDisplayInput();
        // autocomplete library adds this attribute so its a good indicator that autocompleter initialized
        const popupAttr = displayInput.attributes['aria-haspopup'];

        expect(autocomplete).toBeTruthy();
        expect(displayInput).toBeTruthy();
        expect(popupAttr).toBeTruthy();
        expect(popupAttr.value).toEqual('listbox');
    });

    it('should render all address input fields', () => {
        expect(getInputByDataAttribute('data-address-address1')).toBeTruthy();
        expect(getInputByDataAttribute('data-address-address2')).toBeTruthy();
        expect(getInputByDataAttribute('data-address-city')).toBeTruthy();
        expect(getInputByDataAttribute('data-address-stateCode')).toBeTruthy();
        expect(getInputByDataAttribute('data-address-postalCode')).toBeTruthy();
        expect(getInputByDataAttribute('data-address-countryCode')).toBeTruthy();
        expect(getInputByDataAttribute('data-address-longitude')).toBeTruthy();
        expect(getInputByDataAttribute('data-address-latitude')).toBeTruthy();
    });

    it('should allow setting and getting values for address fields', () => {
        const address1 = getInputByDataAttribute('data-address-address1');
        const city = getInputByDataAttribute('data-address-city');
        const stateCode = getInputByDataAttribute('data-address-stateCode');
        const postalCode = getInputByDataAttribute('data-address-postalCode');
        const countryCode = getInputByDataAttribute('data-address-countryCode');
        const longitude = getInputByDataAttribute('data-address-longitude');
        const latitude = getInputByDataAttribute('data-address-latitude');

        address1.value = '123 Main St';
        city.value = 'Springfield';
        stateCode.value = 'IL';
        postalCode.value = '62704';
        countryCode.value = 'US';
        longitude.value = '-89.6500';
        latitude.value = '39.7833';

        expect(address1.value).toBe('123 Main St');
        expect(city.value).toBe('Springfield');
        expect(stateCode.value).toBe('IL');
        expect(postalCode.value).toBe('62704');
        expect(countryCode.value).toBe('US');
        expect(longitude.value).toBe('-89.6500');
        expect(latitude.value).toBe('39.7833');
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
        // autocomplete library adds this attribute so its a good indicator that autocompleter initialized
        const popupAttr = displayInput.attributes['aria-haspopup'];

        expect(autocomplete).toBeTruthy();
        expect(displayInput).toBeTruthy();
        expect(popupAttr).toBeFalsy();
    });
});

describe('autocomplete with no display input', async () => {
    beforeEach(async () => {
        document.body.innerHTML = noDisplayInputAutocompleteHtml;
        await isRendered(getAutocomplete);
    });

    it('should not have custom attributes added', async () => {
        const autocomplete = getAutocomplete();
        const displayInput = getDisplayInput();
        // autocomplete library adds this attribute so its a good indicator that autocompleter initialized
        const popupAttr = displayInput?.attributes['aria-haspopup'];

        expect(autocomplete).toBeTruthy();
        expect(displayInput).toBeFalsy();
        expect(popupAttr).toBeFalsy();
    });
});
