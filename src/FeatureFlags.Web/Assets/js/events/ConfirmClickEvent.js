/**
 * Wrapper for Event for use with confirm dialog.
 */
class ConfirmClickEvent extends Event {
    /**
     * True if event has already been confirmed.
     */
    isConfirmed = false;
}

export default ConfirmClickEvent;
