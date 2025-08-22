/**
 * Enum for identifiers to query DOM elements for the table component.
 * @readonly
 * @enum {string}
 */
const TableElements = Object.freeze({
    Search: 'search',
    FirstPage: 'first-page',
    PreviousPage: 'previous-page',
    NextPage: 'next-page',
    LastPage: 'last-page',
    PerPage: 'per-page',
    Retry: 'retry',
    StartRow: 'start-row',
    EndRow: 'end-row',
    FilteredRows: 'filtered-rows',
    Loading: 'loading',
    Error: 'error',
    Empty: 'empty',
    SortAscTemplate: 'sort-asc-template',
    SortDescTemplate: 'sort-desc-template',
    SortAsc: 'sort-asc',
    SortDesc: 'sort-desc',
    Status: 'status',
    MaxResults: 'max-results',
});

export default TableElements;
