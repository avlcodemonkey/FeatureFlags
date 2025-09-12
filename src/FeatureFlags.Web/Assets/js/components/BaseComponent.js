/**
 * Extend the HTMLElement class to provide common functionality for custom components.
 */
class BaseComponent extends HTMLElement {
    /**
     * Stores commonly queried DOM elements as kvp to improve performance.
     * @type {object}
     */
    #elementCache = {};

    /**
     * Prefix for finding elements via data attributes, and caching elements in memory.
     * @type {string}
     */
    #elementPrefix = '';

    /**
     * Initialize the component.
     * @param {string} key Key for the component, used to identify it.
     */
    constructor(key) {
        super();

        this.#elementPrefix = key ?? '';
        this.#elementCache = {};
    }

    /**
     * Clean up when the component is removed from the document.
     * Don't want to keep references to any elements that have been removed.
     */
    disconnectedCallback() {
        this.#elementCache = {};
    }

    /**
     * Gets the specified DOM element from cache, or finds using querySelector and add to cache.
     * Will match element with attribute `data-{elementPrefix}-{elementKey}`, or `data-{elementKey}` if prefix is empty.
     * @param {string} elementKey Key for the element to find.
     * @returns {HTMLElement|null} Element to find, or null if not found.
     */
    getElement(elementKey) {
        const key = this.#elementPrefix ? `${this.#elementPrefix}-${elementKey}` : elementKey;

        if (!this.#elementCache[key]) {
            const componentName = this.nodeName;
            this.querySelectorAll(`[data-${key}]`).forEach((x) => {
                // handle nested components by searching for matching parent node
                // use a while loop instead of closest() since jsdom closest() doesn't seem to work right
                let el = x;
                while (el.nodeName !== componentName) {
                    // Move up to the parent node
                    el = el.parentNode;
                    if (!el) {
                        break;
                    }
                }
                if (el && el === this) {
                    this.#elementCache[key] = x;
                }
            });
        }
        return this.#elementCache[key];
    }

    /**
     * Sets the prefix for finding elements via data attributes, and caching elements in memory.
     * This is used for testing purposes only. Key should be set via the constructor.
     * @param {string} key Key to set as the prefix for element queries.
     */
    _setKey(key) {
        if (key) {
            this.#elementPrefix = key;
        }
    }
}

// Define the new web component. Should only be used for automated testing.
if ('customElements' in window) {
    customElements.define('nilla-base', BaseComponent);
}

export default BaseComponent;
