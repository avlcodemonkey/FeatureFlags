/**
 * Unit tests for formData util functions.
 */

import {
    describe, expect, it,
} from 'vitest';
import { isRendered, tick } from '../utils';
import { formToObject, objectToForm } from '../../js/utils/formData';

const dataObj = {
    field1: 'value1',
    field2: '2',
    field3: ['option1', 'option2'],
};

const populatedFormHtml = `
    <form>
        <input type="text" name="field1" value="value1" />
        <input type="number" name="field2" value="2" />
        <select name="field3" multiple>
            <option value="option1" selected>option1</option>
            <option value="option2" selected>option2</option>
            <option value="option3">option3</option>
        </select>
    </form>
`;

const unpopulatedFormHtml = `
    <form>
        <input type="text" name="field1" />
        <input type="number" name="field2" />
        <select name="field3" multiple>
            <option value="option1">option1</option>
            <option value="option2">option2</option>
            <option value="option3">option3</option>
        </select>
    </form>
`;

const emptyFormHtml = `
    <form>
    </form>
`;

/**
 * Gets the form element.
 * @returns {HTMLFormElement|null} Form element
 */
function getForm() {
    return document.querySelector('form');
}

describe('formToObject', async () => {
    it('should return populated object when form has data', async () => {
        document.body.innerHTML = populatedFormHtml;
        await isRendered(getForm);

        const result = formToObject(getForm());

        expect(result).toBeTruthy();
        expect(result).toMatchObject(dataObj);
    });

    it('should return empty object when form has no data', async () => {
        document.body.innerHTML = emptyFormHtml;
        await isRendered(getForm);

        const result = formToObject(getForm());

        expect(result).toBeTruthy();
        expect(result).toMatchObject({});
    });
});

describe('objectToForm', async () => {
    it('should populate form when object has data', async () => {
        document.body.innerHTML = unpopulatedFormHtml;
        await isRendered(getForm);
        const form = getForm();

        objectToForm(dataObj, form);
        await tick();
        const { field1, field2, field3 } = form.elements;
        const selectedValues = Array.from(field3.options).filter(option => option.selected).map(option => option.value);

        expect(field1).toBeTruthy();
        expect(field1.value).toEqual(dataObj.field1);

        expect(field2).toBeTruthy();
        expect(field2.value).toEqual(dataObj.field2);

        expect(field3).toBeTruthy();
        expect(selectedValues).toEqual(dataObj.field3);
    });

    it('should do nothing if form or data are missing', async () => {
        document.body.innerHTML = unpopulatedFormHtml;
        await isRendered(getForm);
        const form = getForm();

        objectToForm(null, form);
        await tick();
        const { field1, field2, field3 } = form.elements;
        const selectedValues = Array.from(field3.options).filter(option => option.selected).map(option => option.value);

        expect(field1).toBeTruthy();
        expect(field1.value).toEqual('');

        expect(field2).toBeTruthy();
        expect(field2.value).toEqual('');

        expect(field3).toBeTruthy();
        expect(selectedValues).toEqual([]);
    });
});
