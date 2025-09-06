/**
 * Connects a number input with a range input for usability.
 */
class SliderInput extends HTMLElement {
    constructor() {
        super();

        const rangeInput = this.querySelector('[data-slider-range]');
        const textInput = this.querySelector('[data-slider-text]');
        if (rangeInput && textInput) {
            rangeInput.addEventListener('input', () => {
                textInput.value = rangeInput.value;
            });
            textInput.addEventListener('input', () => {
                rangeInput.value = textInput.value;
            });
        }
    }
}

// Define the new web component
if ('customElements' in window) {
    customElements.define('nilla-slider-input', SliderInput);
}

export default SliderInput;
