import SortDirection from '../constants/SortDirection.js';

/**
 * Table sort settings.
 */
class TableSort {
    /**
     * Name of property to sort on.
     * @type {string}
     */
    property = '';

    /**
     * Direction to sort this property.
     * @type {SortDirection}
     */
    direction = undefined;
}

export default TableSort;
