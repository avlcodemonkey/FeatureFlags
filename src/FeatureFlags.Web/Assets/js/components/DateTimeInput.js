/**
 * Converts a local datetime input to an ISO 8601 string in a hidden input.
 */
class DateTimeInput extends HTMLElement {
    constructor() {
        super();

        const localInput = this.querySelector('[data-datetime-local]');
        const hiddenInput = this.querySelector('[data-datetime-hidden]');
        if (localInput && hiddenInput) {
            const setHiddenValue = () => {
                const date = new Date(localInput.value);
                if (isNaN(date.getTime())) {
                    hiddenInput.value = '';
                } else {
                    hiddenInput.value = date.toISOString();
                }
            };
            setHiddenValue();

            localInput.addEventListener('change', setHiddenValue);
        }
    }
}

// Define the new web component
if ('customElements' in window) {
    customElements.define('nilla-datetime-input', DateTimeInput);
}

export default DateTimeInput;
