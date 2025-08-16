/**
 * Unit tests for formData util functions.
 */

import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import { formToObject, objectToForm } from '../../js/utils/formData.js';
import isRendered from '../testUtils/isRendered.js';
import setupDom from '../testUtils/setupDom.js';
import tick from '../testUtils/tick.js';

// Setup jsdom first
await setupDom();

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

describe('formToObject', () => {
    it('should return populated object when form has data', async () => {
        document.body.innerHTML = populatedFormHtml;
        await isRendered(getForm);

        const result = formToObject(getForm());

        assert.ok(result);
        assert.deepStrictEqual(result, dataObj);
    });

    it('should return empty object when form has no data', async () => {
        document.body.innerHTML = emptyFormHtml;
        await isRendered(getForm);

        const result = formToObject(getForm());

        assert.ok(result);
        assert.deepStrictEqual(result, {});
    });

    it('should ignore unchecked checkboxes and radios', async () => {
        document.body.innerHTML = `
            <form>
                <input type="checkbox" name="cb1" value="yes" />
                <input type="checkbox" name="cb2" value="on" checked />
                <input type="radio" name="r1" value="A" />
                <input type="radio" name="r1" value="B" checked />
            </form>
        `;
        await isRendered(getForm);

        const result = formToObject(getForm());
        assert.deepStrictEqual(result, { cb2: 'on', r1: 'B' });
    });

    it('should ignore fields without a name attribute', async () => {
        document.body.innerHTML = `
            <form>
                <input type="text" value="foo" />
                <input type="text" name="named" value="bar" />
            </form>
        `;
        await isRendered(getForm);

        const result = formToObject(getForm());
        assert.deepStrictEqual(result, { named: 'bar' });
    });

    it('should ignore disabled fields', async () => {
        document.body.innerHTML = `
            <form>
                <input type="text" name="enabled" value="yes" />
                <input type="text" name="disabled" value="no" disabled />
            </form>
        `;
        await isRendered(getForm);

        const result = formToObject(getForm());
        assert.deepStrictEqual(result, { enabled: 'yes' });
    });

    it('should handle malformed HTML gracefully', async () => {
        document.body.innerHTML = `
            <form>
                <input type="text" name="field1" value="abc"
                <input type="number" name="field2" value="123" />
            </form>
        `;
        await isRendered(getForm);

        const result = formToObject(getForm());
        // Only field1 should be present due to malformed field1
        assert.deepStrictEqual(result, { field1: 'abc' });
    });
});

describe('objectToForm', () => {
    it('should populate form when object has data', async () => {
        document.body.innerHTML = unpopulatedFormHtml;
        await isRendered(getForm);
        const form = getForm();

        objectToForm(dataObj, form);
        await tick();
        const { field1, field2, field3 } = form.elements;
        const selectedValues = Array.from(field3.options).filter(option => option.selected).map(option => option.value);

        assert.ok(field1);
        assert.strictEqual(field1.value, dataObj.field1);

        assert.ok(field2);
        assert.strictEqual(field2.value, dataObj.field2);

        assert.ok(field3);
        assert.deepStrictEqual(selectedValues, dataObj.field3);
    });

    it('should do nothing if form or data are missing', async () => {
        document.body.innerHTML = unpopulatedFormHtml;
        await isRendered(getForm);
        const form = getForm();

        objectToForm(null, form);
        await tick();
        const { field1, field2, field3 } = form.elements;
        const selectedValues = Array.from(field3.options).filter(option => option.selected).map(option => option.value);

        assert.ok(field1);
        assert.strictEqual(field1.value, '');

        assert.ok(field2);
        assert.strictEqual(field2.value, '');

        assert.ok(field3);
        assert.deepStrictEqual(selectedValues, []);
    });

    it('should not populate fields without a name attribute', async () => {
        document.body.innerHTML = `
            <form>
                <input type="text" value="" />
                <input type="text" name="named" value="" />
            </form>
        `;
        await isRendered(getForm);
        const form = getForm();

        objectToForm({ named: 'bar', unnamed: 'foo' }, form);
        await tick();

        const { named } = form.elements;
        assert.strictEqual(named.value, 'bar');
        // unnamed field should remain empty
        assert.strictEqual(form.querySelector('input:not([name])').value, '');
    });

    it('should not update disabled fields', async () => {
        document.body.innerHTML = `
            <form>
                <input type="text" name="enabled" value="" />
                <input type="text" name="disabled" value="" disabled />
            </form>
        `;
        await isRendered(getForm);
        const form = getForm();

        objectToForm({ enabled: 'yes', disabled: 'no' }, form);
        await tick();

        assert.strictEqual(form.elements.enabled.value, 'yes');
        assert.strictEqual(form.elements.disabled.value, '');
    });

    it('should ignore extra keys in data object not present in form', async () => {
        document.body.innerHTML = `
            <form>
                <input type="text" name="field1" value="" />
            </form>
        `;
        await isRendered(getForm);
        const form = getForm();

        objectToForm({ field1: 'foo', extra: 'bar' }, form);
        await tick();

        assert.strictEqual(form.elements.field1.value, 'foo');
        // No error should occur for 'extra'
    });
});
