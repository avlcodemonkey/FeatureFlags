import { prettyPrintJson } from '../utils/prettyPrintJson';

/**
 * Web component for displaying json in a pretty way.
 */
class JsonFormatter extends HTMLElement {
    constructor() {
        super();

        try {
            const json = JSON.parse(this.textContent.length ? this.textContent : '{}');
            const html = prettyPrintJson.toHtml(json);
            this.innerHTML = `<pre class="json-container">${html}</pre>`;
        } catch {
            /* empty */
        }
    }
}

// Define the new web component
if ('customElements' in window) {
    customElements.define('nilla-json', JsonFormatter);
}

export default JsonFormatter;
