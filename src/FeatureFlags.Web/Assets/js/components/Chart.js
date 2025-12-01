/* global RequestInit */

import DefaultTimeout from '../constants/Fetch.js';
import BaseComponent from './BaseComponent.js';
import FetchError from '../errors/FetchError.js';
import HttpHeaders from '../constants/HttpHeaders.js';
import ChartElements from '../constants/ChartElements.js';
import ChartTypes from '../constants/ChartTypes.js';
import escape from '../utils/escape.js';

/**
 * @typedef ChartRow
 * @type {object}
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
    #chartUrl = '';

    /**
     * Type of the chart to render.
     * @type {ChartTypes}
     */
    #chartType = '';

    /**
     * Show chart heading.
     * @type {boolean}
     */
    #showHeading = false;

    /**
     * Show labels on the chart.
     * @type {boolean}
     */
    #showLabels = false;

    /**
     * Show primary axis on the chart.
     * @type {boolean}
     */
    #showPrimaryAxis = false;

    /**
     * Show 4 secondary axes on the chart.
     * @type {boolean}
     */
    #showSecondaryAxes = false;

    /**
     * Show data axes on the chart.
     * @type {boolean}
     */
    #showDataAxes = false;

    /**
     * Data spacing for the chart.
     * @type {boolean}
     */
    #dataSpacing = false;

    /**
     * Hide data on the chart.
     * @type {boolean}
     */
    #hideData = false;

    /**
     * Show data on hover.
     * @type {boolean}
     */
    #showDataOnHover = false;

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
     * Display as chart if true, table if false.
     * @type {boolean}
     */
    #viewChart = true;

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

        this.#chartUrl = this.dataset.chartUrl;
        this.#chartType = this.dataset.chartType || ChartTypes.Bar;
        this.#showHeading = (this.dataset.chartShowHeading ?? '').toLowerCase() === 'true';
        this.#showLabels = (this.dataset.chartShowLabels ?? '').toLowerCase() === 'true';
        this.#showPrimaryAxis = (this.dataset.chartShowPrimaryAxis ?? '').toLowerCase() === 'true';
        this.#showSecondaryAxes = (this.dataset.chartShowSecondaryAxes ?? '').toLowerCase() === 'true';
        this.#showDataAxes = (this.dataset.chartShowDataAxes ?? '').toLowerCase() === 'true';
        this.#dataSpacing = (this.dataset.chartDataSpacing ?? '').toLowerCase() === 'true';
        this.#hideData = (this.dataset.chartHideData ?? '').toLowerCase() === 'true';
        this.#showDataOnHover = (this.dataset.chartShowDataOnHover ?? '').toLowerCase() === 'true';

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
        this.getElement(ChartElements.ToggleView)?.addEventListener('click', () => this.#onToggleViewClick());
    }

    /**
     * Fetch data from the server at the URL specified in the `src` property.
     */
    async #fetchData() {
        if (!this.#chartUrl || this.#loading) {
            return;
        }

        this.#loading = true;
        this.#error = false;

        // now request new data
        try {
            let url = this.#chartUrl;

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
                throw new FetchError(`Request to '${this.#chartUrl}' returned invalid response.`);
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
        this.getElement(ChartElements.Table)?.classList.toggle('is-hidden', this.#loading || this.#error || this.#rows.length === 0);

        const updatedAt = this.getElement(ChartElements.UpdatedDate);
        if (updatedAt) {
            updatedAt.dataset.dateValue = this.#updatedDate ? this.#updatedDate.toISOString() : '';
        }
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

        // find largest size from rows
        const maxSize = Math.max(...this.#rows.map(x => typeof x.size !== 'undefined' && x.size !== null && !Number.isNaN(parseFloat(x.size)) ? x.size : 0));

        // Build tbody rows. Each item in #rows is expected to be an object like { valueRaw, start, size, tooltip, label, color }.
        const rowsHtml = this.#rows.map((row) => {
            if (!(typeof row === 'object' && row !== null)) {
                // If row is not an object, try to render as a single-cell row
                return `<tr><th scope="row"></th><td>${escape(row ?? '')}</td></tr>`;
            }

            // Determine header label for this row. Prefer first cell.label if present.
            let headerHtml = `<th scope="row">${escape(row.label)}</th>`;

            // Collect style properties compatible with charts.css (--start, --size, --color)
            const styleParts = [];
            if (typeof row.start !== 'undefined' && row.start !== null && !Number.isNaN(parseFloat(row.start))) {
                styleParts.push(`--start: ${parseFloat(row.start)}`);
            }
            if (typeof row.size !== 'undefined' && row.size !== null) {
                styleParts.push(`--size: calc(${row.size} / ${maxSize})`);
            }
            // allow color from resolved data color or explicit color property
            if (row.color) {
                styleParts.push(`--color: ${escape(row.color)}`);
            }
            const styleAttr = styleParts.length ? ` style="${styleParts.join('; ')}"` : '';

            const tooltipValue = row.tooltip ?? row.size;
            const tooltip = tooltipValue ? `<span class="tooltip">${escape(tooltipValue)}</span>` : '';

            return `<tr>${headerHtml}<td${styleAttr}><span class="data">${escape(String(row.size))}</span>${tooltip}</td></tr>`;
        }).join('\n');

        // @todo add table classes
        const html = `<table class="${this.#buildChartCss()}"><tbody>${rowsHtml}</tbody></table>`;

        table.insertAdjacentHTML('beforeend', html);
    }

    /**
     * Builds the CSS class list for the chart based on the properties.
     * @returns {string} CSS class list
     */
    #buildChartCss() {
        const classList = [
            this.#viewChart ? 'charts-css' : 'striped',
            this.#chartType,
        ];
        if (this.#showHeading) {
            classList.push('show-heading');
        }
        if (this.#showLabels) {
            classList.push('show-labels');
        }
        if (this.#showPrimaryAxis) {
            classList.push('show-primary-axis');
        }
        if (this.#showSecondaryAxes) {
            classList.push('show-4-secondary-axes');
        }
        if (this.#showDataAxes) {
            classList.push('show-data-axes');
        }
        if (this.#dataSpacing) {
            classList.push('data-spacing-15');
        }
        if (this.#hideData) {
            classList.push('hide-data');
        }
        if (this.#showDataOnHover) {
            classList.push('show-data-on-hover');
        }
        return classList.join(' ');
    }

    /**
     * Lets user try to refresh data in case of a failure.
     */
    async #onRefreshClick() {
        if (!this.#loading) {
            await this.#fetchData();
        }
    }

    /**
     * Change the current view mode between chart and table.
     */
    async #onToggleViewClick() {
        if (!this.#loading) {
            this.#viewChart = !this.#viewChart;
            this.#update();
        }
    }
}

// Define the new web component
if ('customElements' in window) {
    customElements.define('nilla-chart', Chart);
}

export default Chart;
