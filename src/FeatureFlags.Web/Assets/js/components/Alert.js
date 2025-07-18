/**
 * Dismissable alert web component.
 */
class Alert extends HTMLElement {
    constructor() {
        super();

        this.querySelectorAll('[data-dismiss]').forEach((btn) => {
            btn.addEventListener('click', this);
        });
    }

    /**
     * Hide the alert when the dismiss button is clicked.
     */
    handleEvent() {
        this.classList.add('is-hidden');
    }
}

// Define the new web component
if ('customElements' in window) {
    customElements.define('nilla-alert', Alert);
}

export default Alert;
