import autocomplete from 'autocompleter';
import FetchError from './FetchError';
import HttpHeaders from '../constants/HttpHeaders';
import HttpMethods from '../constants/HttpMethods';
import DefaultTimeout from '../constants/Fetch';

/**
 * @typedef AutocompleteItem
 * @type {object}
 * @property {string} label Label to display for the item.
 * @property {string} value Value to use when selecting the item.
 */

/**
 * @typedef {import("autocompleter").AutocompleteResult} AutocompleteResult
 */

/**
 * Web component for an input autocomplete.
 */
class Autocomplete extends HTMLElement {
    /** @type {AutocompleteResult} */
    autocompleter;

    constructor() {
        super();

        /**
         * Input shown to the user to interact with.
         * @type {HTMLInputElement}
         */
        const displayInput = this.querySelector('[data-autocomplete-display]');

        /**
         * Hidden input that stores the selected value from the autocomplete
         * @type {HTMLInputElement}
         */
        const valueInput = this.querySelector('[data-autocomplete-value]');

        const { srcUrl, emptyMessage } = this.dataset;
        if (!(srcUrl && displayInput && valueInput)) {
            return;
        }

        this.autocompleter = autocomplete({
            minLength: 2,
            preventSubmit: 2, // PreventSubmit.OnSelect
            emptyMsg: emptyMessage,
            input: displayInput,
            debounceWaitMs: 250,
            async fetch(query, update) {
                if (!query) {
                    update([]);
                    return;
                }

                let suggestions = [];
                try {
                    const headers = {};
                    headers[HttpHeaders.RequestedWith] = 'XMLHttpRequest';

                    const url = new URL(srcUrl, window.location.href);
                    url.search = new URLSearchParams([['query', query]]).toString();

                    const response = await fetch(url, {
                        method: HttpMethods.GET,
                        signal: AbortSignal.timeout(DefaultTimeout),
                        headers,
                    });

                    if (!response.ok) {
                        throw new FetchError(`HTTP ${response.status}: ${response.statusText}`);
                    }

                    /** @type {object[]} */
                    const json = await response.json();
                    if (!(json && Array.isArray(json))) {
                        throw new FetchError(`Request to '${srcUrl}' returned invalid response.`);
                    }

                    suggestions = json ?? [];
                } catch (ex) {
                    console.error(ex);
                    suggestions = [];
                }
                update(suggestions);
            },
            onSelect(/** @type {AutocompleteItem} */ item) {
                valueInput.value = item.value;
                displayInput.value = item.label;
                displayInput.dataset.label = item.label;
            },
        });

        // clear out the value in the hidden input when changing the search value. it'll get re-set in onSelect
        displayInput.addEventListener('change', () => {
            if (displayInput.value !== displayInput.dataset.label) {
                displayInput.value = '';
                valueInput.value = '';
            }
        });
    }

    disconnectedCallback() {
        // this could cause trouble if we later start detaching/reattaching autocompletes from the DOM
        this.autocompleter?.destroy();
    }
}

// Define the new web component
if ('customElements' in window) {
    customElements.define('nilla-autocomplete', Autocomplete);
}

export default Autocomplete;
