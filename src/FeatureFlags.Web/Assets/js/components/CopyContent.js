/**
 * Component to duplicate part of a form and append it to the DOM.
 */
class CopyContent extends HTMLElement {
    constructor() {
        super();

        const btn = this.querySelector('[data-copy-button]');
        if (btn) {
            btn.addEventListener('click', this);
        }
    }

    /**
     * Copy the node when the button is clicked and append it to the DOM.
     */
    async handleEvent() {
        /** @type {HTMLElement} */
        const template = this.querySelector('[data-copy-template]');

        /** @type {HTMLElement} */
        const listContainer = this.querySelector('[data-copy-container]');

        if (template && listContainer) {
            listContainer.insertAdjacentHTML('beforeend', template.innerHTML);
        }
    }
}

// Define the new web component
if ('customElements' in window) {
    customElements.define('nilla-copy-content', CopyContent);
}

export default CopyContent;
