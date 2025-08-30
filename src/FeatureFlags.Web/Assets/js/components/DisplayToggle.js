/**
 * Conditionally show/hide content.
 */
class DisplayToggle extends HTMLElement {
    constructor() {
        super();

        this.querySelectorAll('[data-display-toggle-trigger]').forEach((toggle) => {
            toggle.addEventListener('change', this);
        });
    }

    /**
     * Conditionally show/hide content based on the state of the trigger.
     * @param {Event} event Change event.
     */
    handleEvent(event) {
        /** @type {SelectElement} */
        const selectEl = event.target;
        if (!selectEl) {
            return;
        }

        /** @type {string} */
        const target = selectEl.options[selectEl.selectedIndex]?.dataset.displayToggleTarget;
        if (target) {
            this.querySelectorAll('[data-display-toggle]').forEach((/** @type {HTMLElement} */ el) => {
                if (el.dataset.displayToggle !== target) {
                    el.classList.add('is-hidden');
                } else {
                    el.classList.remove('is-hidden');
                }
            });
        }
    }
}

// Define the new web component
if ('customElements' in window) {
    customElements.define('nilla-display-toggle', DisplayToggle);
}

export default DisplayToggle;
