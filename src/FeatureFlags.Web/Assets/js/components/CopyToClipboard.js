/**
 * Copy to clipboard web component.
 */
class CopyToClipboard extends HTMLElement {
    constructor() {
        super();

        const btn = this.querySelector('[data-copy-button]');
        if (btn) {
            btn.addEventListener('click', this);
        }
    }

    /**
     * Copy the text to clipboard when the button is clicked.
     */
    async handleEvent() {
        const contentInput = this.querySelector('[data-copy-content]');
        if (contentInput) {
            const textToCopy = contentInput.value || contentInput.textContent;
            if (textToCopy) {
                try {
                    await navigator.clipboard.writeText(textToCopy);
                } catch {
                    // do nothing if clipboard write fails
                }
            }
        }
    }
}

// Define the new web component
if ('customElements' in window) {
    customElements.define('nilla-copy', CopyToClipboard);
}

export default CopyToClipboard;
