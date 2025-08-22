/**
 * Searches for a string in the properties of an object.
 * @param {string} search Search string.
 * @param {object} obj Object to search in.
 * @returns {boolean} True if object contains search string, else false.
 */
export default function filterArray(search, obj) {
    const tokens = (search || '').split(' ').map(x => x.toLowerCase());
    const { ...originalObj } = obj;
    return Object.values(originalObj).some((x) => {
        const objVal = `${x}`.toLowerCase();
        return tokens.every(y => objVal.indexOf(y) > -1);
    });
}
