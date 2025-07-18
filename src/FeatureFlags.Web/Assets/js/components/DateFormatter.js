import { format } from 'fecha';

/**
 * Web component for displaying dates in a friendly way.
 */
class DateFormatter extends HTMLElement {
    /** @type {string} */
    dateFormat;

    constructor() {
        super();

        this.dateFormat = this.dataset.dateFormat;
        if (this.textContent) {
            try {
                const date = new Date(this.textContent);
                if (date && date.toString() !== 'Invalid Date') {
                    this.textContent = this.dateFormat ? format(date, this.dateFormat) : date.toLocaleString();
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
