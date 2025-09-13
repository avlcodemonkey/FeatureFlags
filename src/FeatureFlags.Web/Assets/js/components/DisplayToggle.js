import { findClosestComponent } from '../utils/findClosestComponent.js';

/**
 * Conditionally show/hide content.
 */
class DisplayToggle extends HTMLElement {
    constructor() {
        super();

        const componentName = this.nodeName;
        this.querySelectorAll('[data-display-toggle-trigger]').forEach((toggle) => {
            // handle nested components by comparing closest to this
            const node = findClosestComponent(toggle, componentName);
            if (node && node === this) {
                toggle.addEventListener('change', this);
                this.#toggleContent(toggle);
            }
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
     * @param {HTMLSelectElement} selectEl Select element that triggered the change.
     * @private
     */
    #toggleContent(selectEl) {
        /** @type {string} */
        const componentName = this.nodeName;

        /** @type {string} */
        const target = selectEl.options[selectEl.selectedIndex]?.dataset.displayToggleTarget;

        this.querySelectorAll('[data-display-toggle]').forEach((/** @type {HTMLElement} */ el) => {
            // handle nested components by searching for matching parent node
            const node = findClosestComponent(el, componentName);
            if (node && node === this) {
                const toggles = (el.dataset.displayToggle || '').split(',').map(x => x.trim());
                if (!(target && toggles.some(x => x === target))) {
                    el.classList.add('is-hidden');
                } else {
                    el.classList.remove('is-hidden');
                }
            }
        });
    }
}

// Define the new web component
if ('customElements' in window) {
    customElements.define('nilla-display-toggle', DisplayToggle);
}

export default DisplayToggle;
