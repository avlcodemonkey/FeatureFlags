import SortDirection from '../constants/SortDirection.js';
import TableSort from '../models/TableSort.js';

/**
 * Sorts rows based on Sort property.
 * @param {Array<TableSort>} sorts Array of TableSort objects.
 * @param {object} a First element for comparison.
 * @param {object} b Second element for comparison.
 * @returns {number} Negative if a is less than b, positive if a is greater than b, and zero if they are equal
 */
export default function sortedCompare(sorts, a, b) {
    let i = 0;
    const len = sorts.length;
    for (; i < len; i += 1) {
        const sort = sorts[i];
        const aa = a[sort.property];
        const bb = b[sort.property];

        if (aa === null) {
            return 1;
        }
        if (bb === null) {
            return -1;
        }
        if (aa < bb) {
            return sort.direction === SortDirection.Asc ? -1 : 1;
        }
        if (aa > bb) {
            return sort.direction === SortDirection.Asc ? 1 : -1;
        }
    }
    return 0;
}
