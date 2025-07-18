import { prettyPrintJson } from 'pretty-print-json';

/**
 * Web component for displaying json in a pretty way.
 */
class JsonFormatter extends HTMLElement {
    constructor() {
        super();

        try {
            const json = JSON.parse(this.textContent.length ? this.textContent : '{}');
            const html = prettyPrintJson.toHtml(json, {
                indent: 4,
                linkUrls: false,
                quoteKeys: true,
                trailingCommas: false,
            });
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
