import { formatDate } from '../utils/formatDate.js';

/**
 * Web component for displaying dates in a friendly way.
 */
class DateFormatter extends HTMLElement {
    /** @type {string} */
    #dateFormat;

    constructor() {
        super();

        this.#dateFormat = this.dataset.dateFormat;
        this.#updateTextContent(this.textContent);
    }

    static get observedAttributes() {
        return ['data-date-value'];
    }

    attributeChangedCallback(name, oldValue, newValue) {
        if (name === 'data-date-value' && oldValue !== newValue) {
            this.#updateTextContent(newValue);
        }
    }

    #updateTextContent(dateString) {
        if (dateString) {
            try {
                // expect date in UTC. let JS convert to local
                if (!dateString.toLowerCase().endsWith('z') && dateString.indexOf('+') === -1) {
                    dateString = `${dateString}Z`;
                }
                const date = new Date(dateString);
                if (date && date.toString() !== 'Invalid Date') {
                    this.textContent = this.#dateFormat ? formatDate(date, this.#dateFormat) : date.toLocaleString();
                }
            } catch {
                /* empty */
            }
        }
    }
}

// Define the new web component
if ('customElements' in window) {
    customElements.define('nilla-date', DateFormatter);
}

export default DateFormatter;
