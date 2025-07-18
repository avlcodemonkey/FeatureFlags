const entityMap = {
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    '"': '&quot;',
    '\'': '&#39;',
    '/': '&#x2F;',
    '`': '&#x60;',
    '=': '&#x3D;',
};
const escapeRegex = /[&<>"'`=/]/g;

/**
 * Escapes special characters in a string to prevent XSS attacks.
 * @param {string} string The string to escape.
 * @returns {string} The escaped string.
 */
export default function escape(string) {
    return `${string}`.replace(escapeRegex, x => entityMap[x]);
}
