import DefaultTimeout from '../constants/Fetch.js';
import BaseComponent from './BaseComponent.js';
import FetchError from '../errors/FetchError.js';
import HttpHeaders from '../constants/HttpHeaders.js';
import ChartElements from '../constants/ChartElements.js';

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
     * @type {Array<IndexedRow>}
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
        super();

        // set the prefix to use when querying/caching elements
        super.elementPrefix = 'chart';

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
        if (!this.#srcUrl) {
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
        } catch {
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
     * Removes existing table and renders new one.
     */
    #updateChart() {
        const table = this.getElement(ChartElements.Table);
        if (!table) {
            return;
        }
        table.innerHTML = '';

        let html = this.#rows.map((row) => {
            const cells = row.map(x => `<td>${x}</td>`).join('');
            return `<tr>${cells}</tr>`;
        }).join('\n');

        html = `<table>${html}</table>`;

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
