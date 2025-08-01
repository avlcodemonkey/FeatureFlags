/**
 * Convert form object to an object that can be JSON serialized.
 * @param {HTMLFormElement} formElement Form element to convert
 * @returns {object} New object with form data.
 */
function formToObject(formElement) {
    const result = {};

    new FormData(formElement)?.forEach((value, key) => {
        if (!Object.hasOwn(result, key)) {
            result[key] = value;
            return;
        }

        if (!Array.isArray(result[key])) {
            result[key] = [result[key]];
        }
        result[key].push(value);
    });

    return result;
}

/**
 * Use object properties to populate a form.
 * @param {object} data Data to populate into form.
 * @param {HTMLFormElement} formElement Form element to populate
 */
function objectToForm(data, formElement) {
    if (!(data && formElement)) {
        return;
    }

    try {
        Object.entries(data).forEach(([key, value]) => {
            const input = formElement.elements.namedItem(key);
            if (input) {
                if (Array.isArray(value)) {
                    // @ts-ignore input will be a HTMLSelectElement
                    Array.from(input.options).forEach((opt) => {
                        opt.selected = value.includes(opt.value);
                    });
                } else {
                    // @ts-ignore input will be a HTMLInputElement
                    input.value = value;
                }
            }
        });
    } catch {
        /* empty */
    }
}

export { formToObject, objectToForm };
