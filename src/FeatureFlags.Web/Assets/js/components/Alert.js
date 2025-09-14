import { findClosestComponent } from '../utils/findClosestComponent.js';

/**
 * Dismissable alert web component.
 */
class Alert extends HTMLElement {
    constructor() {
        super();

        const componentName = this.nodeName;
        this.querySelectorAll('[data-dismiss]').forEach((btn) => {
            const el = findClosestComponent(btn, componentName);
            if (el && el === this) {
                btn.addEventListener('click', this);
            }
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
