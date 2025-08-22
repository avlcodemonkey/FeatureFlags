/**
 * @typedef IndexedRow
 * @type {object}
 * @property {number} _index Unique identifier for the row.
 */

/**
 * Sorts rows by their original index.
 * @param {IndexedRow} a First row.
 * @param {IndexedRow} b Row to compare first row to.
 * @returns {number} Negative if a is less than b, positive if a is greater than b, and zero if they are equal
 */
export default function indexedCompare(a, b) {
    if (a._index > b._index) {
        return 1;
    }
    return a._index < b._index ? -1 : 0;
}
