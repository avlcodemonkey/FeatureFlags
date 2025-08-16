import ConfirmClickEvent from '../events/ConfirmClickEvent.js';
import BaseDialog from './BaseDialog.js';

/**
 * Web component that wraps confirm logic around any links or buttons it contains.
 */
class ConfirmDialog extends BaseDialog {
    /**
     * Initialize component.
     */
    constructor() {
        super();

        // add event listener for all links and buttons inside the component
        this.querySelectorAll('a, button').forEach(x => x.addEventListener('click', this));
    }

    /**
     * Clean up when removing component.
     */
    disconnectedCallback() {
        super.disconnectedCallback();
    }

    /**
     * Open confirm dialog when user clicks on the element.
     * @param {ConfirmClickEvent} event Click event that triggers the dialog.
     */
    handleEvent(event) {
        if (event.isConfirmed) {
            return;
        }

        const target = this.findTarget(event);
        const { dialogContent, dialogOk, dialogCancel } = target.dataset;
        if (!(dialogContent && dialogOk && dialogCancel)) {
            return;
        }

        event.preventDefault();
        event.stopImmediatePropagation();

        this.showDialog(dialogContent, dialogOk, dialogCancel, (returnValue) => {
            if (returnValue === 'ok') {
                event.isConfirmed = true;

                // re-dispatch event on the original target
                event.target.dispatchEvent(event);
            }
        });
    }
}

// Define the new web component
if ('customElements' in window) {
    customElements.define('nilla-confirm', ConfirmDialog);
}

export default ConfirmDialog;
