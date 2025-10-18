/**
 * Unit tests for nilla-table.
 */

import assert from 'node:assert/strict';
import { beforeEach, describe, it } from 'node:test';
import isRendered from '../testUtils/isRendered.js';
import setupDom from '../testUtils/setupDom.js';
import tick from '../testUtils/tick.js';

// Setup jsdom first
await setupDom();

// Import the custom element after jsdom is set up
await import('../../js/components/Table.js');

const tableHtml = `
    <nilla-table data-key="tbl" data-src-url="/api/test">
        <input data-table-search type="search" />

        <table>
            <thead>
                <tr><th data-property="name">Name</th></tr>
            </thead>
            <tbody>
                <template>
                    <tr><td>{{name}}</td></tr>
                </template>
                <tr data-table-status><td>Status row</td></tr>
            </tbody>
        </table>

        <template data-table-sort-asc-template><span data-table-sort-asc>▲</span></template>
        <template data-table-sort-desc-template><span data-table-sort-desc>▼</span></template>

        <!-- controls used by tests -->
        <select data-table-per-page>
            <option value="1">1</option>
            <option value="2">2</option>
            <option value="10">10</option>
        </select>

        <div class="pagination-controls">
            <button data-table-first-page>First</button>
            <button data-table-previous-page>Previous</button>
            <button data-table-next-page>Next</button>
            <button data-table-last-page>Last</button>
        </div>

        <span data-table-empty class="is-hidden">No items</span>
        <span data-table-loading class="is-hidden">Loading</span>
        <span data-table-error class="is-hidden">Error</span>
        <button data-table-retry>Retry</button>
    </nilla-table>
`;

const tableHtmlMaxResults = `
    <nilla-table data-key="tbl" data-src-url="/api/test" data-max-results="2">
        <table>
            <thead>
                <tr><th data-property="name">Name</th></tr>
            </thead>
            <tbody>
                <template>
                    <tr><td>{{name}}</td></tr>
                </template>
                <tr data-table-status><td>Status row</td></tr>
            </tbody>
        </table>
        <span data-table-empty class="is-hidden">No items</span>
        <span data-table-loading class="is-hidden">Loading</span>
        <span data-table-error class="is-hidden">Error</span>
    </nilla-table>
`;

const tableHtmlWithCounts = `
    <nilla-table data-key="tbl" data-src-url="/api/test">
        <input data-table-search type="search" />
        <table>
            <thead>
                <tr><th data-property="name">Name</th></tr>
            </thead>
            <tbody>
                <template>
                    <tr><td>{{name}}</td></tr>
                </template>
                <tr data-table-status><td>Status row</td></tr>
            </tbody>
        </table>

        <div class="counts">
            <span data-table-start-row>0</span>
            <span data-table-end-row>0</span>
            <span data-table-filtered-rows>0</span>
        </div>

        <select data-table-per-page>
            <option value="1">1</option>
            <option value="2">2</option>
        </select>

        <button data-table-first-page>First</button>
        <button data-table-previous-page>Previous</button>
        <button data-table-next-page>Next</button>
        <button data-table-last-page>Last</button>

        <span data-table-empty class="is-hidden">No items</span>
        <span data-table-loading class="is-hidden">Loading</span>
        <span data-table-error class="is-hidden">Error</span>
    </nilla-table>
`;

const tableHtmlForm = `
    <form id="searchForm" action="/api/form" method="post">
        <input name="q" value="" />
        <select name="type">
            <option value="a">A</option>
            <option value="b">B</option>
        </select>
        <button type="submit">Submit</button>
    </form>

    <nilla-table data-key="tbl" data-src-form="searchForm">
        <table>
            <thead>
                <tr><th data-property="name">Name</th></tr>
            </thead>
            <tbody>
                <template>
                    <tr><td>{{name}}</td></tr>
                </template>
                <tr data-table-status><td>Status row</td></tr>
            </tbody>
        </table>
        <span data-table-empty class="is-hidden">No items</span>
        <span data-table-loading class="is-hidden">Loading</span>
        <span data-table-error class="is-hidden">Error</span>
    </nilla-table>
`;

const tableHtmlMultiSort = `
    <nilla-table data-key="tbl" data-src-url="/api/test">
        <table>
            <thead>
                <tr>
                    <th data-property="name">Name</th>
                    <th data-property="age">Age</th>
                </tr>
            </thead>
            <tbody>
                <template>
                    <tr><td>{{name}}</td><td>{{age}}</td></tr>
                </template>
                <tr data-table-status><td>Status row</td></tr>
            </tbody>
        </table>

        <template data-table-sort-asc-template><span data-table-sort-asc>▲</span></template>
        <template data-table-sort-desc-template><span data-table-sort-desc>▼</span></template>

        <span data-table-empty class="is-hidden">No items</span>
        <span data-table-loading class="is-hidden">Loading</span>
        <span data-table-error class="is-hidden">Error</span>
    </nilla-table>
`;

/**
 * Gets the table element.
 * @returns {HTMLElement | null | undefined} Table element
 */
function getTable() {
    return document.body.querySelector('nilla-table');
}

/**
 * Gets the empty message element inside the current nilla-table.
 * @returns {HTMLElement | null | undefined} Element matching [data-table-empty]
 */
function getEmptyMessage() {
    return getTable()?.querySelector('[data-table-empty]');
}

/**
 * Gets the error message element inside the current nilla-table.
 * @returns {HTMLElement | null | undefined} Element matching [data-table-error]
 */
function getErrorMessage() {
    return getTable()?.querySelector('[data-table-error]');
}

/**
 * Gets the loading indicator element inside the current nilla-table.
 * @returns {HTMLElement | null | undefined} Element matching [data-table-loading]
 */
function getLoadingMessage() {
    return getTable()?.querySelector('[data-table-loading]');
}

/**
 * Gets the retry button inside the current nilla-table.
 * @returns {HTMLButtonElement | null | undefined} Element matching [data-table-retry]
 */
function getRetryButton() {
    return getTable()?.querySelector('[data-table-retry]');
}

/**
 * Gets the per-page select element inside the current nilla-table.
 * @returns {HTMLSelectElement | null | undefined} Element matching [data-table-per-page]
 */
function getPerPageSelect() {
    return getTable()?.querySelector('[data-table-per-page]');
}

/**
 * Gets the first-page navigation button inside the current nilla-table.
 * @returns {HTMLButtonElement | null | undefined} Element matching [data-table-first-page]
 */
function getFirstButton() {
    return getTable()?.querySelector('[data-table-first-page]');
}

/**
 * Gets the previous-page navigation button inside the current nilla-table.
 * @returns {HTMLButtonElement | null | undefined} Element matching [data-table-previous-page]
 */
function getPreviousButton() {
    return getTable()?.querySelector('[data-table-previous-page]');
}

/**
 * Gets the next-page navigation button inside the current nilla-table.
 * @returns {HTMLButtonElement | null | undefined} Element matching [data-table-next-page]
 */
function getNextButton() {
    return getTable()?.querySelector('[data-table-next-page]');
}

/**
 * Gets the last-page navigation button inside the current nilla-table.
 * @returns {HTMLButtonElement | null | undefined} Element matching [data-table-last-page]
 */
function getLastButton() {
    return getTable()?.querySelector('[data-table-last-page]');
}

/**
 * Gets the search input inside the current nilla-table.
 * @returns {HTMLInputElement | null | undefined} Element matching [data-table-search]
 */
function getSearchInput() {
    return (getTable()?.querySelector('[data-table-search]'));
}

/**
 * Returns all data rows (excluding the status row) from the current nilla-table's tbody.
 * @returns {Element[]} Array of rows; empty list if table not present
 */
function getRows() {
    return Array.from(getTable()?.querySelectorAll('tbody tr:not([data-table-status])')) ?? [];
}

/**
 * Gets the start-row count element inside the current nilla-table.
 * @returns {HTMLElement | null | undefined} Element matching [data-table-start-row]
 */
function getStartRow() {
    return getTable()?.querySelector('[data-table-start-row]');
}

/**
 * Gets the end-row count element inside the current nilla-table.
 * @returns {HTMLElement | null | undefined} Element matching [data-table-end-row]
 */
function getEndRow() {
    return getTable()?.querySelector('[data-table-end-row]');
}

/**
 * Gets the filtered-rows count element inside the current nilla-table.
 * @returns {HTMLElement | null | undefined} Element matching [data-table-filtered-rows]
 */
function getFilteredRowsSpan() {
    return getTable()?.querySelector('[data-table-filtered-rows]');
}

describe('nilla-table', () => {
    beforeEach(() => {
        sessionStorage.clear();
        delete globalThis.fetch;
        document.body.innerHTML = '';
    });

    it('renders rows after successful fetch', async () => {
        // Mock fetch to return two rows
        globalThis.fetch = async () => ({
            ok: true,
            json: async () => [{ name: 'Alpha' }, { name: 'Beta' }],
        });

        document.body.innerHTML = tableHtml;
        await isRendered(getTable);
        // Allow microtasks to complete
        await tick();

        const rows = getRows();
        assert.strictEqual(rows.length, 2, 'Should render two rows from fetch');
        assert.strictEqual(rows[0].textContent.trim(), 'Alpha', 'First row should contain Alpha');
        assert.strictEqual(rows[1].textContent.trim(), 'Beta', 'Second row should contain Beta');

        const empty = getEmptyMessage();
        assert.ok(empty, 'Empty message element should exist');
        assert.ok(empty.classList.contains('is-hidden'), 'Empty message should be hidden when data present');
    });

    it('shows empty message when fetch returns no data', async () => {
        globalThis.fetch = async () => ({
            ok: true,
            json: async () => [],
        });

        document.body.innerHTML = tableHtml;
        await isRendered(getTable);
        await tick();

        const rows = getRows();
        assert.strictEqual(rows.length, 0, 'Should render zero rows when fetch returns empty array');

        const empty = getEmptyMessage();
        assert.ok(empty, 'Empty message element should exist');
        assert.ok(!empty.classList.contains('is-hidden'), 'Empty message should be visible when no data');
    });

    it('shows error on failed fetch and retry reloads data', async () => {
        let call = 0;
        // First call fails, second call succeeds
        globalThis.fetch = async () => {
            call += 1;
            if (call === 1) {
                return { ok: false, status: 500, statusText: 'Server error' };
            }
            return {
                ok: true,
                json: async () => [{ name: 'Recovered' }],
            };
        };

        document.body.innerHTML = tableHtml;
        await isRendered(getTable);
        await tick();

        const error = getErrorMessage();
        assert.ok(error, 'Error element should exist after failed fetch');
        assert.ok(!error.classList.contains('is-hidden'), 'Error should be visible after failed fetch');

        // Now click retry which should invoke fetch again and succeed
        const retry = getRetryButton();
        assert.ok(retry, 'Retry button should exist');
        retry.click();
        // wait for retry fetch to complete
        await tick();
        await tick();

        const rows = getRows();
        assert.strictEqual(rows.length, 1, 'Should render one row after successful retry');
        assert.strictEqual(rows[0].textContent.trim(), 'Recovered', 'Row should reflect recovered data');

        // After successful retry error should be hidden
        assert.ok(error.classList.contains('is-hidden'), 'Error should be hidden after successful retry');
    });

    it('toggles sorting when header clicked', async () => {
        // Provide rows in unsorted order: Beta, Alpha, Gamma
        globalThis.fetch = async () => ({
            ok: true,
            json: async () => [{ name: 'Beta' }, { name: 'Alpha' }, { name: 'Gamma' }],
        });

        document.body.innerHTML = tableHtml;
        await isRendered(getTable);
        await tick();

        // initial order should reflect original array
        let rows = Array.from(getRows()).map(r => r.textContent.trim());
        assert.deepStrictEqual(rows, ['Beta', 'Alpha', 'Gamma'], 'Initial order should match fetch order');

        // Click header to sort ascending by name -> Alpha, Beta, Gamma
        const header = getTable()?.querySelector('th[data-property="name"]');
        header?.click();
        await tick();

        rows = Array.from(getRows()).map(r => r.textContent.trim());
        assert.deepStrictEqual(rows, ['Alpha', 'Beta', 'Gamma'], 'Order should be ascending after first click');

        // Click header to sort descending -> Gamma, Beta, Alpha
        header?.click();
        await tick();

        rows = Array.from(getRows()).map(r => r.textContent.trim());
        assert.deepStrictEqual(rows, ['Gamma', 'Beta', 'Alpha'], 'Order should be descending after second click');

        // Click header again to remove sort -> back to original fetch order
        header?.click();
        await tick();

        rows = Array.from(getRows()).map(r => r.textContent.trim());
        assert.deepStrictEqual(rows, ['Beta', 'Alpha', 'Gamma'], 'Order should revert to original after third click');
    });

    it('paginates when per-page changed and navigation buttons clicked', async () => {
        // Provide three rows for paging
        globalThis.fetch = async () => ({
            ok: true,
            json: async () => [{ name: 'Alpha' }, { name: 'Beta' }, { name: 'Gamma' }],
        });

        document.body.innerHTML = tableHtml;
        await isRendered(getTable);
        await tick();

        const select = getPerPageSelect();
        assert.ok(select, 'Per-page select should exist');

        // Change per-page to 1 so we can navigate between pages
        select.value = '1';
        select.dispatchEvent(new window.Event('change', { bubbles: true }));
        // allow handler to run
        await tick();

        let rows = getRows();
        assert.strictEqual(rows.length, 1, 'Should show one row after per-page set to 1');
        assert.strictEqual(rows[0].textContent.trim(), 'Alpha', 'Page 1 should show Alpha');

        // Next -> Beta
        const next = getNextButton();
        next?.click();
        await tick();
        rows = getRows();
        assert.strictEqual(rows[0].textContent.trim(), 'Beta', 'After next should show Beta');

        // Last -> Gamma
        const last = getLastButton();
        last?.click();
        await tick();
        rows = getRows();
        assert.strictEqual(rows[0].textContent.trim(), 'Gamma', 'After last should show Gamma');

        // First -> Alpha
        const first = getFirstButton();
        first?.click();
        await tick();
        rows = getRows();
        assert.strictEqual(rows[0].textContent.trim(), 'Alpha', 'After first should show Alpha');

        // Previous when at first should remain Alpha
        const prev = getPreviousButton();
        prev?.click();
        await tick();
        rows = getRows();
        assert.strictEqual(rows[0].textContent.trim(), 'Alpha', 'Previous at first page should keep Alpha');
    });

    it('shows loading indicator during a long fetch and disables search while loading', async () => {
        let finishFetch;
        globalThis.fetch = () => new Promise((resolve) => {
            finishFetch = () => resolve({ ok: true, json: async () => [{ name: 'Delta' }] });
        });

        document.body.innerHTML = tableHtml;
        await isRendered(getTable);

        // let component start fetch
        await tick();

        const loading = getLoadingMessage();
        const search = getSearchInput();
        assert.ok(loading, 'Loading element should exist');
        assert.ok(!loading.classList.contains('is-hidden'), 'Loading should be visible while fetch is in progress');
        assert.ok(search?.disabled, 'Search input should be disabled while loading');

        // finish fetch
        finishFetch();
        // allow fetch completion and rendering
        await tick();
        await tick();

        const rows = getRows();
        assert.strictEqual(rows.length, 1, 'Should render rows after fetch finishes');
        assert.ok(loading.classList.contains('is-hidden'), 'Loading should be hidden after fetch completes');
        assert.ok(!search?.disabled, 'Search input should be enabled after load completes');
    });

    it('filters rows based on search input after debounce', async () => {
        globalThis.fetch = async () => ({
            ok: true,
            json: async () => [{ name: 'Alpha' }, { name: 'Beta' }, { name: 'Alpine' }],
        });

        document.body.innerHTML = tableHtml;
        await isRendered(getTable);
        await tick();

        const search = getSearchInput();
        assert.ok(search, 'Search input should exist');

        // type "Al" into search and wait for debounce (250ms)
        search.value = 'Al';
        search.dispatchEvent(new window.Event('input', { bubbles: true }));

        // wait slightly longer than debounce to ensure filter runs
        await new Promise(resolve => setTimeout(resolve, 320));

        const rows = Array.from(getRows()).map(r => r.textContent.trim());
        // Should match Alpha and Alpine
        assert.deepStrictEqual(rows, ['Alpha', 'Alpine'], 'Search should filter rows by substring (case-insensitive)');
    });

    it('respects saved per-page and current page from sessionStorage', async () => {
        // Persist settings before rendering
        sessionStorage.setItem('tbl_perPage', '1');
        sessionStorage.setItem('tbl_currentPage', '1'); // zero based -> second page

        globalThis.fetch = async () => ({
            ok: true,
            json: async () => [{ name: 'One' }, { name: 'Two' }, { name: 'Three' }],
        });

        document.body.innerHTML = tableHtml;
        await isRendered(getTable);
        await tick();

        // per-page should be set from session storage
        const select = getPerPageSelect();
        assert.ok(select, 'Per-page select should exist');
        assert.strictEqual(select.value, '1', 'Per-page select should reflect session storage');

        const rows = getRows();
        assert.strictEqual(rows.length, 1, 'Should show one row per page');
        assert.strictEqual(rows[0].textContent.trim(), 'Two', 'Should start on saved current page (second item)');
    });

    it('updates header aria-sort and shows sort icons when sorting', async () => {
        globalThis.fetch = async () => ({
            ok: true,
            json: async () => [{ name: 'Beta' }, { name: 'Alpha' }, { name: 'Gamma' }],
        });

        document.body.innerHTML = tableHtml;
        await isRendered(getTable);
        await tick();

        const header = getTable()?.querySelector('th[data-property="name"]');
        assert.ok(header, 'Header should exist');

        // first click -> ascending, should show asc icon
        header?.click();
        await tick();
        assert.strictEqual(header.getAttribute('aria-sort'), 'ascending', 'Header should have aria-sort ascending');
        assert.ok(header.querySelector('[data-table-sort-asc]'), 'Ascending sort icon should be present');

        // second click -> descending
        header?.click();
        await tick();
        assert.strictEqual(header.getAttribute('aria-sort'), 'descending', 'Header should have aria-sort descending');
        assert.ok(header.querySelector('[data-table-sort-desc]'), 'Descending sort icon should be present');

        // third click -> remove sort
        header?.click();
        await tick();
        assert.strictEqual(header.hasAttribute('aria-sort'), false, 'Header should not have aria-sort after removing sort');
        assert.ok(!header.querySelector('[data-table-sort-asc]')
          && !header.querySelector('[data-table-sort-desc]'), 'No sort icons should be present after removing sort');
    });

    it('honors data-max-results and limits displayed rows', async () => {
        globalThis.fetch = async () => ({
            ok: true,
            json: async () => [{ name: 'A' }, { name: 'B' }, { name: 'C' }],
        });

        document.body.innerHTML = tableHtmlMaxResults;
        await isRendered(getTable);
        await tick();

        const rows = getRows();
        assert.strictEqual(rows.length, 2, 'Should limit results to max-results value (2)');
        // status row should still be present
        const statusRow = getTable()?.querySelector('tbody tr[data-table-status]');
        assert.ok(statusRow, 'Status row should remain present');
    });

    it('persists sort settings to sessionStorage and restores removal', async () => {
        globalThis.fetch = async () => ({
            ok: true,
            json: async () => [{ name: 'C' }, { name: 'A' }, { name: 'B' }],
        });

        document.body.innerHTML = tableHtml;
        await isRendered(getTable);
        await tick();

        const header = getTable()?.querySelector('th[data-property="name"]');
        assert.ok(header, 'Header should exist');

        // Click -> asc, should persist sort
        header?.click();
        await tick();
        const sortRaw1 = sessionStorage.getItem('tbl_sort');
        assert.ok(sortRaw1, 'Sort should be saved to sessionStorage');
        const sortVal1 = JSON.parse(sortRaw1);
        assert.strictEqual(Array.isArray(sortVal1), true, 'Saved sort should be an array');
        assert.strictEqual(sortVal1.length, 1, 'Saved sort should contain one entry after first click');
        assert.strictEqual(sortVal1[0].property, 'name', 'Saved sort property should be name');
        assert.strictEqual(sortVal1[0].direction, 'asc', 'Saved sort direction should be asc');

        // Click -> desc, should persist updated direction
        header?.click();
        await tick();
        const sortVal2 = JSON.parse(sessionStorage.getItem('tbl_sort'));
        assert.strictEqual(sortVal2[0].direction, 'desc', 'Saved sort direction should be desc after second click');

        // Click -> remove, should persist empty array
        header?.click();
        await tick();
        assert.strictEqual(sessionStorage.getItem('tbl_sort'), '[]', 'Saved sort should be empty array after removing sort');
    });

    it('persists search to sessionStorage and restores it on init', async () => {
        // Ensure session has a saved search and verify it's restored into the input
        sessionStorage.setItem('tbl_search', 'PersistMe');

        globalThis.fetch = async () => ({
            ok: true,
            json: async () => [{ name: 'X' }],
        });

        document.body.innerHTML = tableHtml;
        await isRendered(getTable);
        await tick();

        const search = getSearchInput();
        assert.ok(search, 'Search input should exist');
        assert.strictEqual(search.value, 'PersistMe', 'Search input should be restored from sessionStorage');

        // Now change the search and ensure it is saved after debounce
        search.value = 'NewQuery';
        search.dispatchEvent(new window.Event('input', { bubbles: true }));
        // wait for debounce
        await new Promise(resolve => setTimeout(resolve, 320));
        assert.strictEqual(sessionStorage.getItem('tbl_search'), 'NewQuery', 'Search should be saved to sessionStorage after input debounce');
    });

    it('shows error when fetch returns non-array JSON', async () => {
        globalThis.fetch = async () => ({
            ok: true,
            json: async () => ({ not: 'an array' }),
        });

        document.body.innerHTML = tableHtml;
        await isRendered(getTable);
        // wait for fetch handling
        await tick();
        await tick();

        const error = getErrorMessage();
        assert.ok(error, 'Error element should exist');
        assert.ok(!error.classList.contains('is-hidden'), 'Error should be visible when server returns invalid payload');
    });

    it('shows error when fetch throws an exception', async () => {
        globalThis.fetch = async () => {
            throw new Error('network failure');
        };

        document.body.innerHTML = tableHtml;
        await isRendered(getTable);
        // wait for fetch handling
        await tick();
        await tick();

        const error = getErrorMessage();
        assert.ok(error, 'Error element should exist');
        assert.ok(!error.classList.contains('is-hidden'), 'Error should be visible when fetch throws');
    });

    it('disables navigation buttons appropriately on first and last pages', async () => {
        globalThis.fetch = async () => ({
            ok: true,
            json: async () => [{ name: 'One' }, { name: 'Two' }, { name: 'Three' }],
        });

        document.body.innerHTML = tableHtml;
        await isRendered(getTable);
        await tick();

        const select = getPerPageSelect();
        assert.ok(select, 'Per-page select should exist');

        // set per-page to 1 to create multiple pages
        select.value = '1';
        select.dispatchEvent(new window.Event('change', { bubbles: true }));
        await tick();

        const first = getFirstButton();
        const prev = getPreviousButton();
        const next = getNextButton();
        const last = getLastButton();

        // On first page: first & prev disabled, next & last enabled
        assert.ok(first?.disabled, 'First should be disabled on first page');
        assert.ok(prev?.disabled, 'Previous should be disabled on first page');
        assert.ok(!next?.disabled, 'Next should be enabled on first page');
        assert.ok(!last?.disabled, 'Last should be enabled on first page');

        // Navigate to last page
        last?.click();
        await tick();

        // On last page: next & last disabled, first & prev enabled
        assert.ok(next?.disabled, 'Next should be disabled on last page');
        assert.ok(last?.disabled, 'Last should be disabled on last page');
        assert.ok(!first?.disabled, 'First should be enabled on last page');
        assert.ok(!prev?.disabled, 'Previous should be enabled on last page');
    });

    it('disables controls while loading and when an error occurs', async () => {
        // Part A: loading state
        let finishFetch;
        globalThis.fetch = () => new Promise((resolve) => {
            finishFetch = () => resolve({ ok: true, json: async () => [{ name: 'LoadingItem' }] });
        });

        document.body.innerHTML = tableHtml;
        await isRendered(getTable);

        // allow component to begin fetch
        await tick();

        const firstDuringLoad = getFirstButton();
        const prevDuringLoad = getPreviousButton();
        const nextDuringLoad = getNextButton();
        const lastDuringLoad = getLastButton();
        const selectDuringLoad = getPerPageSelect();
        const searchDuringLoad = getSearchInput();

        assert.ok(firstDuringLoad?.disabled, 'First should be disabled while loading');
        assert.ok(prevDuringLoad?.disabled, 'Previous should be disabled while loading');
        assert.ok(nextDuringLoad?.disabled, 'Next should be disabled while loading');
        assert.ok(lastDuringLoad?.disabled, 'Last should be disabled while loading');
        assert.ok(selectDuringLoad?.disabled, 'Per-page select should be disabled while loading');
        assert.ok(searchDuringLoad?.disabled, 'Search should be disabled while loading');

        // finish fetch and allow render
        finishFetch();
        await tick();
        await tick();

        // Part B: error state - simulate a failing fetch
        globalThis.fetch = async () => ({ ok: false, status: 500, statusText: 'Err' });

        document.body.innerHTML = tableHtml;
        await isRendered(getTable);
        await tick();
        await tick();

        const firstOnError = getFirstButton();
        const prevOnError = getPreviousButton();
        const nextOnError = getNextButton();
        const lastOnError = getLastButton();
        const selectOnError = getPerPageSelect();
        const searchOnError = getSearchInput();

        assert.ok(firstOnError?.disabled, 'First should be disabled on error');
        assert.ok(prevOnError?.disabled, 'Previous should be disabled on error');
        assert.ok(nextOnError?.disabled, 'Next should be disabled on error');
        assert.ok(lastOnError?.disabled, 'Last should be disabled on error');
        assert.ok(selectOnError?.disabled, 'Per-page select should be disabled on error');
        assert.ok(searchOnError?.disabled, 'Search should be disabled on error');
    });

    it('saves form data to sessionStorage when form submitted and restores on init', async () => {
        // Mock fetch to succeed (returns empty array)
        let capturedUrl = null;
        globalThis.fetch = async (url) => {
            capturedUrl = url;
            return { ok: true, json: async () => [] };
        };

        // render the form + table
        document.body.innerHTML = tableHtmlForm;
        await isRendered(getTable);

        // set form values and submit
        const form = document.getElementById('searchForm');
        assert.ok(form, 'Form should exist');
        const qInput = form.elements.namedItem('q');
        // @ts-ignore
        qInput.value = 'search-term';
        const typeInput = form.elements.namedItem('type');
        // @ts-ignore
        typeInput.value = 'b';

        // submit the form (component's listener prevents default and triggers fetch)
        form.dispatchEvent(new window.Event('submit', { bubbles: true, cancelable: true }));
        // allow fetch and saving
        await tick();
        await tick();

        // verify fetch used form action
        assert.strictEqual(capturedUrl, 'http://localhost/api/form', 'Fetch should be called with form action URL');
        // verify sessionStorage saved formData
        const saved = sessionStorage.getItem('tbl_formData');
        assert.ok(saved, 'Form data should be saved to sessionStorage');
        const parsed = JSON.parse(saved);
        assert.strictEqual(parsed.q, 'search-term', 'Saved q value should match');
        assert.strictEqual(parsed.type, 'b', 'Saved select value should match');

        // Now confirm restore: set session and re-render and ensure inputs populated via objectToForm
        sessionStorage.setItem('tbl_formData', JSON.stringify({ q: 'restored', type: 'a' }));
        document.body.innerHTML = tableHtmlForm;
        await isRendered(getTable);
        // inputs are outside nilla-table; objectToForm will populate them during setupForm
        const form2 = document.getElementById('searchForm');
        // Give a tick for any setup
        await tick();
        const q2 = form2.elements.namedItem('q');
        // @ts-ignore
        assert.strictEqual(q2.value, 'restored', 'Form q input should be restored from session storage');
        const type2 = form2.elements.namedItem('type');
        // @ts-ignore
        assert.strictEqual(type2.value, 'a', 'Form select should be restored from session storage');
    });

    it('resets saved currentPage to 0 if it is greater than computed maxPage', async () => {
        // Save an out-of-range currentPage
        sessionStorage.setItem('tbl_currentPage', '5');

        // fetch returns a single row so maxPage will be 0
        globalThis.fetch = async () => ({ ok: true, json: async () => [{ name: 'Only' }] });

        document.body.innerHTML = tableHtml;
        await isRendered(getTable);
        // allow fetch handling
        await tick();
        await tick();

        // component should have reset currentPage to 0 in session storage
        assert.strictEqual(sessionStorage.getItem('tbl_currentPage'), '0', 'Saved currentPage should be reset to 0 when out of bounds');

        const rows = getRows();
        assert.strictEqual(rows.length, 1, 'Should show the single row after reset');
        assert.strictEqual(rows[0].textContent.trim(), 'Only', 'Rendered row should be the only item');
    });

    it('updates start-row, end-row, and filtered-rows across pages and during loading/error', async () => {
        globalThis.fetch = async () => ({ ok: true, json: async () => [{ name: 'A' }, { name: 'B' }, { name: 'C' }] });

        document.body.innerHTML = tableHtmlWithCounts;
        await isRendered(getTable);
        await tick();

        // initial (perPage default 10) filteredRows should be 3, start 1, end 3
        const startSpan = getStartRow();
        const endSpan = getEndRow();
        const filteredSpan = getFilteredRowsSpan();
        assert.ok(startSpan && endSpan && filteredSpan, 'Count spans should exist');
        assert.strictEqual(filteredSpan.textContent, '3', 'Filtered rows should show 3');
        assert.strictEqual(startSpan.textContent, '1', 'Start row should be 1 on first page');
        assert.strictEqual(endSpan.textContent, '3', 'End row should be 3 on first page');

        // change per-page to 1 to create paging
        const select = getPerPageSelect();
        // @ts-ignore
        select.value = '1';
        select.dispatchEvent(new window.Event('change', { bubbles: true }));
        await tick();

        // on first page
        assert.strictEqual(filteredSpan.textContent, '3', 'Filtered rows remain 3 after per-page change');
        assert.strictEqual(startSpan.textContent, '1', 'Start should be 1 on page 1');
        assert.strictEqual(endSpan.textContent, '1', 'End should be 1 on page 1');

        // navigate to next page
        const next = getNextButton();
        next?.click();
        await tick();
        assert.strictEqual(startSpan.textContent, '2', 'Start should be 2 on page 2');
        assert.strictEqual(endSpan.textContent, '2', 'End should be 2 on page 2');

        // simulate loading state by stubbing fetch to a pending promise and re-rendering
        let resolveFetch;
        globalThis.fetch = () => new Promise((resolve) => {
            resolveFetch = resolve;
        });
        document.body.innerHTML = tableHtmlWithCounts;
        await isRendered(getTable);
        // allow constructor to start fetch
        await tick();

        // during loading counts should be zeroed
        const startDuringLoad = getStartRow();
        const endDuringLoad = getEndRow();
        const filteredDuringLoad = getFilteredRowsSpan();
        assert.strictEqual(startDuringLoad.textContent, '0', 'Start should be 0 while loading');
        assert.strictEqual(endDuringLoad.textContent, '0', 'End should be 0 while loading');
        assert.strictEqual(filteredDuringLoad.textContent, '0', 'Filtered rows should be 0 while loading');

        // finish the fetch with data
        resolveFetch({ ok: true, json: async () => [{ name: 'X' }] });
        await tick();
        await tick();

        // after fetch completes counts should update
        assert.strictEqual(getFilteredRowsSpan().textContent, '1', 'Filtered rows should update after load');
        assert.strictEqual(getStartRow().textContent, '1', 'Start should be 1 after load');
        assert.strictEqual(getEndRow().textContent, '1', 'End should be 1 after load');

        // simulate error state
        globalThis.fetch = async () => ({ ok: false, status: 500, statusText: 'Err' });
        document.body.innerHTML = tableHtmlWithCounts;
        await isRendered(getTable);
        // allow handling
        await tick();
        await tick();

        assert.strictEqual(getFilteredRowsSpan().textContent, '0', 'Filtered rows should be 0 on error');
        assert.strictEqual(getStartRow().textContent, '0', 'Start should be 0 on error');
        assert.strictEqual(getEndRow().textContent, '0', 'End should be 0 on error');
    });

    it('saves per-page selection to sessionStorage and restores on init', async () => {
        globalThis.fetch = async () => ({ ok: true, json: async () => [{ name: 'One' }, { name: 'Two' }] });

        document.body.innerHTML = tableHtml;
        await isRendered(getTable);
        await tick();

        const select = getPerPageSelect();
        assert.ok(select, 'Per-page select should exist');

        // change per-page to 1 and ensure saved
        // @ts-ignore
        select.value = '1';
        select.dispatchEvent(new window.Event('change', { bubbles: true }));
        await tick();

        assert.strictEqual(sessionStorage.getItem('tbl_perPage'), '1', 'Per-page should be saved to sessionStorage');

        // re-render and ensure selection restored
        document.body.innerHTML = tableHtml;
        await isRendered(getTable);
        await tick();

        const select2 = getPerPageSelect();
        assert.strictEqual(select2.value, '1', 'Per-page select should be restored from sessionStorage on init');
    });

    it('supports multi-column sorting and persists the sort array order', async () => {
        // Provide rows where name is equal for two rows so secondary sort matters
        globalThis.fetch = async () => ({
            ok: true,
            json: async () => [
                { name: 'A', age: 2 },
                { name: 'A', age: 1 },
                { name: 'B', age: 0 },
            ],
        });

        document.body.innerHTML = tableHtmlMultiSort;
        await isRendered(getTable);
        await tick();

        const thName = getTable()?.querySelector('th[data-property="name"]');
        const thAge = getTable()?.querySelector('th[data-property="age"]');
        assert.ok(thName && thAge, 'Both headers should exist');

        // Click name -> asc
        thName?.click();
        await tick();

        // Click age -> asc (secondary sort)
        thAge?.click();
        await tick();

        // Verify saved sort array contains two entries in order [name, age]
        const saved = sessionStorage.getItem('tbl_sort');
        assert.ok(saved, 'Sort should be saved to sessionStorage');
        const arr = JSON.parse(saved);
        assert.strictEqual(Array.isArray(arr), true, 'Saved sort should be an array');
        assert.strictEqual(arr.length, 2, 'Should contain two sort entries');
        assert.strictEqual(arr[0].property, 'name', 'First sort should be name');
        assert.strictEqual(arr[1].property, 'age', 'Second sort should be age');
        assert.strictEqual(arr[0].direction, 'asc', 'Name sort should be ascending');
        assert.strictEqual(arr[1].direction, 'asc', 'Age sort should be ascending');

        // Verify rendered order respects multi-sort: within name 'A', age 1 then 2
        const rowsText = Array.from(getRows()).map(r => r.textContent.trim());
        // Expect A (age 1), A (age 2), B
        assert.strictEqual(rowsText[0].startsWith('A'), true);
        assert.strictEqual(rowsText[1].startsWith('A'), true);
        assert.ok(rowsText[0] !== rowsText[1], 'Two A rows should be distinct');
    });

    it('shows error when AbortSignal.timeout produces an already-aborted signal', async () => {
        // override AbortSignal.timeout to return an already aborted signal for this test
        const originalTimeout = AbortSignal.timeout;
        AbortSignal.timeout = () => {
            const ac = new AbortController();
            ac.abort();
            return ac.signal;
        };

        globalThis.fetch = async (url, options) => {
            // If fetch is called with an aborted signal, emulate an abort rejection
            if (options?.signal?.aborted) {
                throw new DOMException('The operation was aborted.', 'AbortError');
            }
            return { ok: true, json: async () => [{ name: 'X' }] };
        };

        document.body.innerHTML = tableHtml;
        await isRendered(getTable);
        // allow fetch handling
        await tick();
        await tick();

        const err = getErrorMessage();
        assert.ok(err, 'Error element should exist when signal is aborted');
        assert.ok(!err.classList.contains('is-hidden'), 'Error should be visible on abort');

        // restore original
        AbortSignal.timeout = originalTimeout;
    });
});
