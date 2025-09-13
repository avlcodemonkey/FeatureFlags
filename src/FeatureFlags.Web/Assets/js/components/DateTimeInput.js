import { formatDate } from '../utils/formatDate.js';

/**
 * Connects local datetime input value to UTC for a hidden input and vice-versa on load.
 */
class DateTimeInput extends HTMLElement {
    constructor() {
        super();

        const localInput = this.querySelector('[data-datetime-local]');
        const hiddenInput = this.querySelector('[data-datetime-hidden]');
        if (localInput && hiddenInput) {
            if (hiddenInput.value && !localInput.value) {
                // convert utc time to local value for date input to use
                const date = new Date(hiddenInput.value);
                if (isNaN(date.getTime())) {
                    localInput.value = '';
                } else {
                    // input expects date in format "yyyy-MM-ddThh:mm" followed by optional ":ss" or ":ss.SSS".
                    localInput.value = formatDate(date, 'YYYY-MM-DDTHH:mm:ss'); // .toISOString().replace('Z', '');
                }
            };

            localInput.addEventListener('change', () => {
                if (!localInput.value) {
                    hiddenInput.value = '';
                    return;
                }

                // convert value from date input to UTC for saving
                const date = new Date(localInput.value);
                if (isNaN(date.getTime())) {
                    hiddenInput.value = '';
                } else {
                    hiddenInput.value = date.toISOString();
                }
            });
        }
    }
}

// Define the new web component
if ('customElements' in window) {
    customElements.define('nilla-datetime-input', DateTimeInput);
}

export default DateTimeInput;
