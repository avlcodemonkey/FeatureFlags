import escape from './escape.js';

// regex for handling ifs
const ifRegex = /{{#if\s*(!)?(\w+)}}([\s\S]+?){{\/if}}/m;

/**
 * Converts a template string with replacement values into usable HTML.
 * @param {string} template A template string containing replacement values and conditional blocks.
 * @param {object} data An object whose keys match the replacement strings in your template.
 * @param {boolean} shouldEscape If true escape HTML characters in the replacement values.
 * @returns {string} Updated template string with replacements made.
 * @description Supports simple replacements using `{{key}}` syntax, and conditional blocks using `{{#if condition}}...{{/if}}` syntax.
 *              No nested properties or conditionals.
 */
export default function renderTemplate(template, data, shouldEscape = true) {
    let result = template ?? '';
    let match;

    // if the template is empty, return an empty string
    if (result === '') {
        return '';
    }
    // if the data is not an object, return the template as is
    if (typeof data !== 'object' || data === null) {
        return result;
    }

    // eslint-disable-next-line no-cond-assign
    while (match = ifRegex.exec(result)) {
        // if the match is not null, negate the if
        const notIf = !!match[1];

        // get the condition from the match, removing whitespace, and check if true
        let isTrue = data[match[2].trim()];
        if (notIf) {
            // if the match is negated flip the value
            isTrue = !isTrue;
        }

        // if the conditions are met, remove the if statements.  if not remove the whole block
        result = result.replace(match[0], isTrue ? match[3] : '');
    }

    // loop through each property of the data
    Object.keys(data).forEach((key) => {
        let value = data[key] === undefined || data[key] === null ? '' : `${data[key]}`;
        if (shouldEscape) {
            value = escape(value);
        }
        result = result.replaceAll(`{{${key}}}`, value);
    });

    return result;
}
