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
        if (this.textContent) {
            try {
                // backend will always return date in UTC. let JS convert to local
                let dateString = this.textContent;
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
