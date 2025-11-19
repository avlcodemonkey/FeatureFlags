import DefaultTimeout from '../constants/Fetch.js';
import BaseComponent from './BaseComponent.js';
import FetchError from '../errors/FetchError.js';
import HttpHeaders from '../constants/HttpHeaders.js';
import ChartElements from '../constants/ChartElements.js';

/**
 * @typedef ChartRow
 * @type {object}
 * @property {string} value Display value
 * @property {number} start Start position for bar
 * @property {number} size Size of bar
 * @property {string} tooltip Tooltip text
 * @property {string} label Label for the row
 * @property {string} color Color for the bar
 */

/**
 * Web component for rendering chart data.
 */
class Chart extends BaseComponent {
    /**
     * Url to request data from the server.
     * @type {string}
     */
    #srcUrl = '';

    /**
     * Data fetched from the server.
     * @type {Array<ChartRow>}
     */
    #rows = [];

    /**
     * Date and time data was fetched from the server.
     * @type {Date|null}
     */
    #updatedDate = null;

    /**
     * Indicates data is currently being fetched from the server if true.
     * @type {boolean}
     */
    #loading = false;

    /**
     * Indicates an error occurred fetching data from the server if true.
     * @type {boolean}
     */
    #error = false;

    /**
     * Initialize chart by fetching data from the server and rendering HTML.
     */
    constructor() {
        super('chart');

        this.#srcUrl = this.dataset.srcUrl;

        this.#setupFooter();
    }

    /**
     * Fetches data to populate the table once the component is connected to the DOM.
     */
    connectedCallback() {
        // fetchData is async but we can fire & forget here
        this.#fetchData();
    }

    /**
     * Clean up when removing component.
     */
    disconnectedCallback() {
        super.disconnectedCallback();
    }

    /**
     * Add event handler for the button for refreshing data.
     */
    #setupFooter() {
        this.getElement(ChartElements.Refresh)?.addEventListener('click', () => this.#onRefreshClick());
    }

    /**
     * Fetch data from the server at the URL specified in the `src` property.
     */
    async #fetchData() {
        if (!this.#srcUrl || this.#loading) {
            return;
        }

        this.#loading = true;
        this.#error = false;

        // first clear out the existing data and update the table
        this.#rows = [];
        this.#update();

        // now request new data
        try {
            let url = this.#srcUrl;

            const headers = {};
            headers[HttpHeaders.RequestedWith] = 'XMLHttpRequest';

            const options = /** @type {RequestInit} */ ({
                headers,
                signal: AbortSignal.timeout(DefaultTimeout),
            });

            const response = await fetch(url, options);
            if (!response.ok) {
                throw new FetchError(`HTTP ${response.status}: ${response.statusText}`);
            }

            /** @type {object[] } */
            const json = await response.json();
            if (!(json && Array.isArray(json))) {
                throw new FetchError(`Request to '${this.#srcUrl}' returned invalid response.`);
            }

            this.#rows = json ?? [];
        } catch (e) {
            console.log(e);
            this.#rows = [];
            this.#error = true;
        } finally {
            this.#loading = false;
            this.#updatedDate = new Date();
        }

        this.#update();
    }

    /**
     * Updates the DOM based on the chart properties.
     */
    #update() {
        this.#updateStatus();
        this.#updateChart();
    }

    /**
     * Shows/hides the chart loading and error indicators.
     */
    #updateStatus() {
        this.getElement(ChartElements.Loading)?.classList.toggle('is-hidden', !this.#loading);
        this.getElement(ChartElements.Error)?.classList.toggle('is-hidden', !this.#error);
        this.getElement(ChartElements.Empty)?.classList.toggle('is-hidden', this.#loading || this.#error || this.#rows.length !== 0);

        const updatedAt = this.getElement(ChartElements.UpdatedDate);
        if (updatedAt) {
            // @todo localization
            updatedAt.textContent = this.#updatedDate ? this.#updatedDate.toLocaleString() : '';
        }
    }

    /**
     * Escape HTML to reduce XSS risk when inserting raw values.
     * @param {string} str
     * @returns {string}
     */
    #escapeHtml(str) {
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    /**
     * Removes existing table and renders new one.
     *
     * Builds HTML compatible with the structure rendered by `chart.vue`.
     * Supports legacy simple arrays of values as well as the richer value objects
     * produced by the charts mapping (objects containing valueRaw, start, size, tooltip, label, datasetName, color, etc).
     */
    #updateChart() {
        const table = this.getElement(ChartElements.Table);
        if (!table) {
            return;
        }
        table.innerHTML = '';

        // if no rows, nothing to render
        if (!this.#rows || this.#rows.length === 0) {
            return;
        }

        // Build tbody rows. Each item in #rows is expected to be an object like { valueRaw, start, size, tooltip, label, color }.
        const rowsHtml = this.#rows.map((row) => {
            if (!(typeof row === 'object' && row !== null)) {
                // If row is not an object, try to render as a single-cell row
                return `<tr><th scope="row"></th><td>${this.#escapeHtml(row ?? '')}</td></tr>`;
            }

            // Determine header label for this row. Prefer first cell.label if present.
            let headerHtml = `<th scope="row">${this.#escapeHtml(row.label)}</th>`;

            const tooltip = row.tooltip ? `<span class="tooltip">${this.#escapeHtml(row.tooltip)}</span>` : '';

            // Collect style properties compatible with charts.css (--start, --size, --color)
            const styleParts = [];
            if (typeof row.start !== 'undefined' && row.start !== null && !Number.isNaN(parseFloat(row.start))) {
                styleParts.push(`--start: ${parseFloat(row.start)}`);
            }
            if (typeof row.size !== 'undefined' && row.size !== null && !Number.isNaN(parseFloat(row.size))) {
                styleParts.push(`--size: ${parseFloat(row.size)}`);
            }
            // allow color from resolved data color or explicit color property
            if (row.color) {
                styleParts.push(`--color: ${this.#escapeHtml(row.color)}`);
            }

            const styleAttr = styleParts.length ? ` style="${styleParts.join('; ')}"` : '';

            return `<tr>${headerHtml}<td${styleAttr}><span class="data">${this.#escapeHtml(String(row.value))}</span>${tooltip}</td></tr>`;
        }).join('\n');

        // @todo add table classes
        const html = `<table class="charts-css"><tbody>${rowsHtml}</tbody></table>`;

        table.insertAdjacentHTML('beforeend', html);
    }

    /**
     * Lets user try to refresh data in case of a failure.
     */
    async #onRefreshClick() {
        if (!this.#loading) {
            await this.#fetchData();
        }
    }
}

// Define the new web component
if ('customElements' in window) {
    customElements.define('nilla-chart', Chart);
}

export default Chart;
