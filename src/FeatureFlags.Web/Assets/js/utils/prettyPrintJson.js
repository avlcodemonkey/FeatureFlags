/**
 * Pretty prints JSON data as syntax-highlighted HTML. Subset of functionality from
 * pretty-print-json v3.0.5 ~~ https://pretty-print-json.js.org ~~ MIT License
 */

/**
 * Regex to match HTML special characters and escaped quotes for safe output.
 * @type {RegExp}
 */
const invalidHtml = /[<>&]|\\"/g;

/**
 * Regex to match lines in pretty-printed JSON for further processing.
 * @type {RegExp}
 */
const jsonLine = /^( *)("[^"]+": )?("[^"]*"|[\w.+-]*)?([{}[\],]*)?$/mg;

/**
 * Escapes special HTML characters for safe rendering.
 * @param {string} char - Character to escape.
 * @returns {string} Escaped HTML entity.
 */
const toHtml = (char) => {
    let result;
    if (char === '<') {
        result = '&lt;';
    } else if (char === '>') {
        result = '&gt;';
    } else if (char === '&') {
        result = '&amp;';
    } else {
        result = '&bsol;&quot;';
    }
    return result;
};

/**
 * Wraps a value in a span with a class for syntax highlighting.
 * @param {string} type - JSON value type (e.g., 'key', 'string', 'number').
 * @param {string} display - Value to display inside the span.
 * @returns {string} HTML span string or empty string if display is falsy.
 */
const spanTag = (type, display) => display ? '<span class="json-' + type + '">' + display + '</span>' : '';

/**
 * Builds HTML for a JSON value with appropriate syntax highlighting.
 * @param {string} value - JSON value as a string.
 * @returns {string} HTML string for the value.
 */
const buildValueHtml = (value) => {
    const strType = value.startsWith('"') && 'string';
    const boolType = ['true', 'false'].includes(value) && 'boolean';
    const nullType = value === 'null' && 'null';
    const type = boolType || nullType || strType || 'number';
    return spanTag(type, value);
};

/**
 * Default options for prettyPrintJson.
 * @type {{ indent: number, quoteKeys: boolean, trailingCommas: boolean }}
 */
const defaults = {
    indent: 4,
    quoteKeys: true,
    trailingCommas: false,
};

/**
 * Utility for pretty-printing JSON as syntax-highlighted HTML.
 * @namespace
 */
const prettyPrintJson = {
    /**
     * Converts a JavaScript value to pretty-printed, syntax-highlighted HTML.
     * @param {object} data - Data to pretty-print (object, array, etc).
     * @param {object} [options] - Formatting options.
     * @returns {string} HTML string representing the pretty-printed JSON.
     */
    toHtml(data, options) {
        const settings = { ...defaults, ...options };

        /**
         * Replaces each JSON line with syntax-highlighted HTML.
         * @param {string} match - Full matched line.
         * @param {...string} parts - Regex capture groups: indent, key, value, end.
         * @returns {string} HTML for the line.
         */
        const replacer = (match, ...parts) => {
            const part = { indent: parts[0], key: parts[1], value: parts[2], end: parts[3] };
            const findName = settings.quoteKeys ? /(.*)(): / : /"([\w$]+)": |(.*): /;
            const indentHtml = part.indent || '';
            const keyName = part.key?.replace(findName, '$1$2');
            const keyHtml = part.key ? spanTag('key', keyName) + spanTag('mark', ': ') : '';
            const valueHtml = part.value ? buildValueHtml(part.value) : '';
            const noComma = !part.end || [']', '}'].includes(match.at(-1));
            const addComma = settings.trailingCommas && match.at(0) === ' ' && noComma;
            const endHtml = spanTag('mark', addComma ? (part.end ?? '') + ',' : part.end);
            return indentHtml + keyHtml + valueHtml + endHtml;
        };

        const json = JSON.stringify(data, null, settings.indent) || 'undefined';
        const html = json.replace(invalidHtml, toHtml).replace(jsonLine, replacer);

        return html;
    },
};

export { prettyPrintJson };
