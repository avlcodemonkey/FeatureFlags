/**
 * Conditionally show/hide content.
 */
class DisplayToggle extends HTMLElement {
    constructor() {
        super();

        this.querySelectorAll('[data-display-toggle-trigger]').forEach((toggle) => {
            toggle.addEventListener('change', this);
            this.#toggleContent(toggle);
        });
    }

    /**
     * Conditionally show/hide content based on the state of the trigger.
     * @param {Event} event Change event.
     */
    handleEvent(event) {
        /** @type {HTMLSelectElement} */
        const selectEl = event.target;
        if (!selectEl) {
            return;
        }

        this.#toggleContent(selectEl);
    }

    /**
     * Show/hide content based on the selected target.
     * @param {HTMLSelectElement} selectEl
     * @private
     */
    #toggleContent(selectEl) {
        /** @type {string} */
        const target = selectEl.options[selectEl.selectedIndex]?.dataset.displayToggleTarget;
        this.querySelectorAll('[data-display-toggle]').forEach((/** @type {HTMLElement} */ el) => {
            if (!target || el.dataset.displayToggle !== target) {
                el.classList.add('is-hidden');
            } else {
                el.classList.remove('is-hidden');
            }
        });
    }
}

// Define the new web component
if ('customElements' in window) {
    customElements.define('nilla-display-toggle', DisplayToggle);
}

export default DisplayToggle;
