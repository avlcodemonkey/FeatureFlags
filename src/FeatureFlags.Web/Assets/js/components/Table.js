/* global RequestInit */

import DefaultTimeout from '../constants/Fetch.js';
import HttpHeaders from '../constants/HttpHeaders.js';
import TableSettings from '../constants/TableSettings.js';
import SortDirection from '../constants/SortDirection.js';
import FetchError from '../errors/FetchError.js';
import { formToObject, objectToForm } from '../utils/formData.js';
import renderTemplate from '../utils/renderTemplate.js';
import BaseComponent from './BaseComponent.js';
import indexedCompare from '../utils/indexedCompare.js';
import sortedCompare from '../utils/sortedCompare.js';
import filterArray from '../utils/filterArray.js';
import TableElements from '../constants/TableElements.js';
import TableSort from '../models/TableSort.js';

/**
 * @typedef IndexedRow
 * @type {object}
 * @property {number} _index Unique identifier for the row.
 */

/**
 * Web component for rendering table data.
 */
class Table extends BaseComponent {
    /**
     * Unique identifier for this table.
     * @type {string}
     */
    #key = '';

    /**
     * Url to request data from the server.
     * @type {string}
     */
    #srcUrl = '';

    /**
     * ID for form to include with request.
     * @type {string}
     */
    #srcForm = '';

    /**
     * Max number of results to display.
     * @type {number|undefined}
     */
    #maxResults = undefined;

    /**
     * Data fetched from the server.
     * @type {Array<IndexedRow>}
     */
    #rows = [];

    /**
     * Data from the server filtered based on current display settings.
     * @type {Array<IndexedRow>}
     */
    #filteredRows = [];

    /**
     * Sort settings for the table.
     * @type {Array<TableSort>}
     */
    #tableSorts = [];

    /**
     * Total number of rows based on current table filters.
     * @type {number}
     */
    #filteredRowTotal = 0;

    /**
     * Current page number.
     * @type {number}
     */
    #currentPage = 0;

    /**
     * Number of rows to display per page.
     * @type {number}
     */
    #perPage = 10;

    /**
     * Total number of pages.
     * @type {number}
     */
    #maxPage = 0;

    /**
     * String to filter data for.
     * @type {string}
     */
    #search = '';

    /**
     * Tracks the timeout for re-filtering data.
     * @type {number}
     */
    #debounceTimer = 0;

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
     * Initialize table by loading settings from sessionStorage, processing HTML to add event handlers, and fetching data from the server.
     */
    constructor() {
        super('table');

        this.#key = this.dataset.key;
        this.#srcUrl = this.dataset.srcUrl;
        this.#srcForm = this.dataset.srcForm;

        this.#maxResults = Number(this.dataset.maxResults);
        if (Number.isNaN(this.#maxResults)) {
            this.#maxResults = undefined;
        }

        // check sessionStorage for saved settings
        this.#perPage = Number(this.#loadSetting(TableSettings.PerPage) ?? '10');
        this.#currentPage = Number(this.#loadSetting(TableSettings.CurrentPage) ?? '0');
        this.#search = this.#loadSetting(TableSettings.Search) ?? '';
        this.#tableSorts = JSON.parse(this.#loadSetting(TableSettings.Sort) ?? '[]');

        this.#setupHeader();
        this.#setupFooter();
        this.#setupStatus();
        this.#setupSorting();
        this.#setupForm();
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
     * Sets the default value and adds event handler for the search input.
     */
    #setupHeader() {
        const searchInput = /** @type {HTMLInputElement} */ (this.getElement(TableElements.Search));
        if (searchInput) {
            searchInput.value = this.#search;
            searchInput.addEventListener('input', (/** @type {InputEvent} */ e) => this.#onSearchInput(e));
            searchInput.focus();
        }
    }

    /**
     * Add event handlers for the buttons for moving between pages.
     */
    #setupFooter() {
        this.getElement(TableElements.FirstPage)?.addEventListener('click', () => this.#setPage(0));
        this.getElement(TableElements.PreviousPage)?.addEventListener('click', () => this.#setPage(Math.max(this.#currentPage - 1, 0)));
        this.getElement(TableElements.NextPage)?.addEventListener('click', () => this.#setPage(Math.min(this.#currentPage + 1, this.#maxPage)));
        this.getElement(TableElements.LastPage)?.addEventListener('click', () => this.#setPage(this.#maxPage));

        const tablePerPageSelect = /** @type {HTMLSelectElement} */ (this.getElement(TableElements.PerPage));
        if (tablePerPageSelect) {
            tablePerPageSelect.value = `${this.#perPage}`;
            tablePerPageSelect.addEventListener('change', (/** @type {InputEvent} */ e) => this.#onPerPageChange(e));
        }
    }

    /**
     * Add event handlers for table status functionality.
     */
    #setupStatus() {
        this.getElement(TableElements.Retry)?.addEventListener('click', () => this.#onRetryClick());
    }

    /**
     * Add event handlers for table sorting functionality.
     */
    #setupSorting() {
        const headers = this.querySelectorAll('th[data-property]');
        for (const th of headers) {
            th.addEventListener('click', () => this.#onSortClick(th));
        }
    }

    /**
     * Add event handlers for form submission.
     */
    #setupForm() {
        if (!this.#srcForm) {
            return;
        }

        const formElement = /** @type {HTMLFormElement} */ (document.getElementById(this.#srcForm));
        if (!formElement) {
            return;
        }

        const json = this.#loadSetting(TableSettings.FormData);
        if (json) {
            try {
                const data = JSON.parse(json);
                if (data) {
                    objectToForm(data, formElement);
                }
            } catch {
                /* empty */
            }
        }

        formElement.addEventListener('submit', (/** @type {SubmitEvent} */ e) => {
            // make sure the form doesn't actually submit
            e.preventDefault();
            e.stopImmediatePropagation();

            if (!this.#loading) {
                this.#fetchData();
            }
        });
    }

    /**
     * Fetch data from the server at the URL specified in the `src` property.
     */
    async #fetchData() {
        if (!(this.#srcUrl || this.#srcForm)) {
            return;
        }

        this.#loading = true;
        this.#error = false;

        // save the current page for later so we can try to navigate to it after reloading data
        const originalPage = this.#currentPage;

        // first clear out the existing data and update the table
        this.#rows = [];
        this.#filterData();

        // now request new data
        try {
            let url = this.#srcUrl;

            const headers = {};
            headers[HttpHeaders.RequestedWith] = 'XMLHttpRequest';

            const options = /** @type {RequestInit} */ ({
                headers,
                signal: AbortSignal.timeout(DefaultTimeout),
            });

            if (this.#srcForm) {
                const formElement = /** @type {HTMLFormElement} */ (document.getElementById(this.#srcForm));
                if (formElement) {
                    url = formElement.action;
                    options.body = new FormData(formElement);
                    options.method = formElement.method;

                    this.#saveSetting(TableSettings.FormData, JSON.stringify(formToObject(formElement)));
                }
            }

            const response = await fetch(url, options);
            if (!response.ok) {
                throw new FetchError(`HTTP ${response.status}: ${response.statusText}`);
            }

            /** @type {object[] } */
            const json = await response.json();
            if (!(json && Array.isArray(json))) {
                throw new FetchError(`Request to '${this.#srcUrl}' returned invalid response.`);
            }

            this.#rows = json.map((x, index) => ({ ...x, _index: index })) ?? [];
        } catch (ex) {
            console.error(ex);
            this.#rows = [];
            this.#error = true;
        } finally {
            this.#loading = false;
        }

        // try to navigate to the original page after reloading data
        if (originalPage) {
            this.#currentPage = originalPage;
            this.#saveSetting(TableSettings.CurrentPage, this.#currentPage);
        }

        this.#filterData();
    }

    /**
     * Updates the DOM based on the table properties.
     */
    #update() {
        this.#updateSearch();
        this.#updateRowNumbers();
        this.#updatePageButtons();
        this.#updateStatus();
        this.#updateSortHeaders();
        this.#updateRows();
    }

    /**
     * Enable/disable search input.
     */
    #updateSearch() {
        const searchInput = /** @type {HTMLInputElement} */ (this.getElement(TableElements.Search));
        if (searchInput) {
            searchInput.disabled = this.#loading || this.#error;
        }
    }
    /**
     * Updates the start, end, and total numbers.
     */
    #updateRowNumbers() {
        const startRowSpan = this.getElement(TableElements.StartRow);
        if (startRowSpan) {
            const startRowNum = this.#filteredRowTotal ? (this.#currentPage * this.#perPage) + 1 : 0;
            startRowSpan.textContent = `${this.#loading || this.#error ? 0 : startRowNum}`;
        }

        const endRowSpan = this.getElement(TableElements.EndRow);
        if (endRowSpan) {
            const endRowNum = this.#filteredRowTotal ? Math.min((this.#currentPage + 1) * this.#perPage, this.#filteredRowTotal) : 0;
            endRowSpan.textContent = `${this.#loading || this.#error ? 0 : endRowNum}`;
        }

        const filteredRowsSpan = this.getElement(TableElements.FilteredRows);
        if (filteredRowsSpan) {
            filteredRowsSpan.textContent = `${this.#loading || this.#error ? 0 : this.#filteredRowTotal}`;
        }

        const maxResultsSpan = this.getElement(TableElements.MaxResults);
        if (maxResultsSpan) {
            maxResultsSpan.classList.toggle('is-hidden', !this.#maxResults || this.#rows.length <= this.#maxResults);
        }
    }

    /**
     * Enable/disable paging buttons.
     */
    #updatePageButtons() {
        const shouldDisable = this.#loading || this.#error;

        const firstPageButton = /** @type {HTMLButtonElement} */ (this.getElement(TableElements.FirstPage));
        if (firstPageButton) {
            firstPageButton.disabled = shouldDisable || this.#currentPage === 0;
        }

        const previousPageButton = /** @type {HTMLButtonElement} */ (this.getElement(TableElements.PreviousPage));
        if (previousPageButton) {
            previousPageButton.disabled = shouldDisable || this.#currentPage === 0;
        }

        const nextPageButton = /** @type {HTMLButtonElement} */ (this.getElement(TableElements.NextPage));
        if (nextPageButton) {
            nextPageButton.disabled = shouldDisable || this.#currentPage === this.#maxPage;
        }

        const lastPageButton = /** @type {HTMLButtonElement} */ (this.getElement(TableElements.LastPage));
        if (lastPageButton) {
            lastPageButton.disabled = shouldDisable || this.#currentPage === this.#maxPage;
        }

        const tablePerPageSelect = /** @type {HTMLSelectElement} */ (this.getElement(TableElements.PerPage));
        if (tablePerPageSelect) {
            tablePerPageSelect.disabled = shouldDisable;
        }
    }

    /**
     * Shows/hides the table loading and error indicators.
     */
    #updateStatus() {
        this.getElement(TableElements.Loading)?.classList.toggle('is-hidden', !this.#loading);
        this.getElement(TableElements.Error)?.classList.toggle('is-hidden', !this.#error);
        this.getElement(TableElements.Empty)?.classList.toggle('is-hidden', this.#loading || this.#error || this.#filteredRowTotal !== 0);
    }

    /**
     * Updates table headers to show correct sorting icons.
     */
    #updateSortHeaders() {
        const sortAscTemplate = this.getElement(TableElements.SortAscTemplate);
        const sortDescTemplate = this.getElement(TableElements.SortDescTemplate);
        if (!(sortAscTemplate && sortDescTemplate)) {
            return;
        }

        const headers = this.querySelectorAll('th[data-property]');
        for (const th of headers) {
            const { property } = th.dataset;
            const sortOrder = this.#sortOrder(property);
            const sortAsc = th.querySelector(`[data-table-${TableElements.SortAsc}]`);
            const sortDesc = th.querySelector(`[data-table-${TableElements.SortDesc}]`);

            // make sure the TH has the correct sort icon
            if (sortOrder === SortDirection.Asc) {
                sortDesc?.remove();
                if (!sortAsc) {
                    th.insertAdjacentHTML('beforeend', sortAscTemplate.innerHTML);
                }
                th.setAttribute('aria-sort', 'ascending');
            } else if (sortOrder === SortDirection.Desc) {
                sortAsc?.remove();
                if (!sortDesc) {
                    th.insertAdjacentHTML('beforeend', sortDescTemplate.innerHTML);
                }
                th.setAttribute('aria-sort', 'descending');
            } else {
                sortAsc?.remove();
                sortDesc?.remove();
                th.removeAttribute('aria-sort');
            }
        }
    }

    /**
     * Determines which type of sorting to use to display sort icon.
     * @param {string} property Property to find sort icon for.
     * @returns {SortDirection|undefined} Order of sorting for this property.
     */
    #sortOrder(property) {
        const index = property ? this.#tableSorts.findIndex(x => x.property === property) : -1;
        return index === -1 ? undefined : this.#tableSorts[index].direction;
    }

    /**
     * Removes existing table rows and renders new rows using the filtered data.
     */
    #updateRows() {
        const rows = this.querySelectorAll(`tbody tr:not([data-table-${TableElements.Status}])`);
        for (const x of rows) {
            x.remove();
        }

        const tbody = this.querySelector('tbody');
        if (!tbody) {
            throw new DOMException('Table body missing.');
        }
        const template = tbody.querySelector('template');
        if (!template) {
            throw new DOMException('Table row template missing.');
        }

        const html = this.#filteredRows.map(row => renderTemplate(template.innerHTML, row)).join('\n');
        tbody.insertAdjacentHTML('beforeend', html);
    }

    /**
     * Load a value from session storage.
     * @param {string} name Key to store the value as.
     * @returns {string} Value from session storage if found, else null.
     */
    #loadSetting(name) {
        return sessionStorage.getItem(`${this.#key}_${name}`);
    }

    /**
     * Saves a value to session storage.
     * @param {string} name Key to save the value to.
     * @param {string|number} value Value to save to storage.
     */
    #saveSetting(name, value) {
        sessionStorage.setItem(`${this.#key}_${name}`, value.toString());
    }

    /**
     * Create a new array of results, filter by the search query, and sort.
     */
    #filterData() {
        let filteredData = this.#search
            ? (this.#rows?.filter(filterArray.bind(null, this.#search.toLowerCase())) ?? [])
            : [...(this.#rows ?? [])];

        if (this.#maxResults) {
            filteredData = filteredData.slice(0, this.#maxResults);
        }

        // sort the new array
        if (filteredData.length) {
            filteredData.sort(this.#tableSorts?.length ? sortedCompare.bind(null, this.#tableSorts) : indexedCompare);
        }

        // cache the total number of filtered records and max number of pages for paging
        this.#filteredRowTotal = filteredData.length;
        this.#maxPage = Math.max(Math.ceil(this.#filteredRowTotal / this.#perPage) - 1, 0);
        if (this.#currentPage > this.#maxPage) {
            this.#currentPage = 0;
            this.#saveSetting(TableSettings.CurrentPage, 0);
        }

        // determine the correct slice of data for the current page, and reassign our array to trigger the update
        this.#filteredRows = filteredData.slice(this.#perPage * this.#currentPage, (this.#perPage * this.#currentPage) + this.#perPage);

        this.#update();
    }

    /**
     * Handle input in search box and filter data.
     * @param {InputEvent} event Input event to get search value from.
     */
    #onSearchInput(event) {
        if (this.#loading || this.#error) {
            return;
        }

        const target = /** @type {HTMLInputElement} */ (event?.target);
        const newValue = target?.value;
        if (this.#debounceTimer) {
            clearTimeout(this.#debounceTimer);
        }
        this.#debounceTimer = window.setTimeout(() => {
            if (this.#search !== newValue) {
                this.#currentPage = 0;
                this.#saveSetting(TableSettings.CurrentPage, 0);
            }

            this.#search = newValue;
            this.#saveSetting(TableSettings.Search, newValue);

            this.#filterData();
        }, 250);
    }

    /**
     * Handle input for per page select box and update data to display.
     * @param {InputEvent} event Input event to get per page value from.
     */
    #onPerPageChange(event) {
        if (this.#loading || this.#error) {
            return;
        }

        const target = /** @type {HTMLSelectElement} */ (event?.target);
        const newVal = Number(target?.value ?? '10');
        if (this.#perPage !== newVal) {
            this.#currentPage = 0;
            this.#saveSetting(TableSettings.CurrentPage, 0);
        }

        this.#perPage = newVal;
        this.#saveSetting(TableSettings.PerPage, newVal);

        this.#filterData();
    }

    /**
     * Move to specific page of table.
     * @param {number} page Page to move to, zero based.
     */
    #setPage(page) {
        if (this.#loading || this.#error) {
            return;
        }

        this.#currentPage = page < 0 || page > this.#maxPage ? 0 : page;
        this.#saveSetting(TableSettings.CurrentPage, this.#currentPage);
        this.#filterData();
    }

    /**
     * Handle click in table header to sort by a column.
     * @param {HTMLElement} elem TH that was clicked on.
     */
    #onSortClick(elem) {
        if (this.#loading || this.#error) {
            return;
        }

        const { property } = elem.dataset;
        if (!property) {
            return;
        }

        const index = this.#tableSorts.findIndex(x => x.property === property);
        if (index === -1) {
            this.#tableSorts.push({ property, direction: SortDirection.Asc });
        } else if (this.#tableSorts[index].direction === SortDirection.Asc) {
            this.#tableSorts[index].direction = SortDirection.Desc;
        } else {
            this.#tableSorts = this.#tableSorts.filter(x => x.property !== property);
        }

        this.#saveSetting(TableSettings.Sort, JSON.stringify(this.#tableSorts));

        this.#filterData();
    }

    /**
     * Lets user try to reload data in case of a failure.
     */
    async #onRetryClick() {
        if (!this.#loading) {
            await this.#fetchData();
        }
    }
}

// Define the new web component
if ('customElements' in window) {
    customElements.define('nilla-table', Table);
}

export default Table;
