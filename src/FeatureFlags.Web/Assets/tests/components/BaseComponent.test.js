/**
 * Unit tests for nilla-info.
 * Testing keyboard events is iffy, so focus/tab trap logic isn't tested.
 */

import {
    beforeEach, describe, expect, it, vi,
} from 'vitest';
import { isRendered, tick } from '../utils';
import '../../js/components/BaseComponent';

const textElement = 'first element';
const keyElement = 'first-element';
const textOutsideElement = 'outside element';
const keyOutsideElement = 'outside-element';
const textPrefixedElement = 'prefixed 1st element';
const keyPrefixedElement = '1st-element';

const html = `
    <nilla-base>
        <div data-${keyElement}>${textElement}</div>
        <div data-prefixed-${keyPrefixedElement}>${textPrefixedElement}</div>
    </nilla-base>
    <div data-${keyOutsideElement}>${textOutsideElement}</div>
`;

/**
 * Gets the base component custom element.
 * @returns {HTMLElement | null | undefined} Base component element
 */
function getBaseComponent() {
    return document.body.querySelector('nilla-base');
}

describe('BaseComponent with no prefix', async () => {
    beforeEach(async () => {
        document.body.innerHTML = html;
        await isRendered(getBaseComponent);
    });

    it('should query element only once', async () => {
        const component = getBaseComponent();
        const spy = vi.spyOn(component, 'querySelector');

        const firstGet = component.getElement(keyElement);
        // run twice to confirm that the second result comes from the cache instead of using querySelector
        const secondGet = component.getElement(keyElement);
        const len = Object.keys(component.elementCache).length;

        expect(component.elementPrefix).toBeFalsy();
        expect(firstGet).toBeTruthy();
        expect(firstGet.innerHTML).toEqual(textElement);
        expect(secondGet).toBeTruthy();
        expect(secondGet.innerHTML).toEqual(textElement);
        expect(len).toEqual(1);
        expect(spy).toHaveBeenCalledTimes(1);
    });

    it('should not find element outside component', async () => {
        const component = getBaseComponent();
        const spy = vi.spyOn(component, 'querySelector');

        const firstGet = component.getElement(keyOutsideElement);
        const len = Object.keys(component.elementCache).length;

        expect(firstGet).toBeFalsy();
        expect(len).toEqual(0);
        expect(spy).toHaveBeenCalledTimes(1);
    });

    it('should not find element with prefix', async () => {
        const component = getBaseComponent();
        const spy = vi.spyOn(component, 'querySelector');

        const firstGet = component.getElement(keyPrefixedElement);
        const len = Object.keys(component.elementCache).length;

        expect(firstGet).toBeFalsy();
        expect(len).toEqual(0);
        expect(spy).toHaveBeenCalledTimes(1);
    });

    it('should empty cache when removed from dom', async () => {
        const component = getBaseComponent();
        const spy = vi.spyOn(component, 'querySelector');

        const firstGet = component.getElement(keyElement);
        const firstLen = Object.keys(component.elementCache).length;
        const secondGet = component.getElement(`prefixed-${keyPrefixedElement}`);
        const secondLen = Object.keys(component.elementCache).length;

        // remove the component from the dom and trigger the disconnectedCallback
        document.body.innerHTML = '';
        await tick();
        const thirdLen = Object.keys(component.elementCache).length;

        expect(firstGet).toBeTruthy();
        expect(firstLen).toEqual(1);
        expect(secondGet).toBeTruthy();
        expect(secondLen).toEqual(2);
        expect(spy).toHaveBeenCalledTimes(2);
        expect(thirdLen).toEqual(0);
    });
});

describe('BaseComponent with prefix', async () => {
    beforeEach(async () => {
        document.body.innerHTML = html;
        await isRendered(getBaseComponent);
    });

    it('should query element only once', async () => {
        const component = getBaseComponent();
        component.elementPrefix = 'prefixed';
        const spy = vi.spyOn(component, 'querySelector');

        const firstGet = component.getElement(keyPrefixedElement);
        // run twice to confirm that the second result comes from the cache instead of using querySelector
        const secondGet = component.getElement(keyPrefixedElement);
        const len = Object.keys(component.elementCache).length;

        expect(firstGet).toBeTruthy();
        expect(firstGet.innerHTML).toEqual(textPrefixedElement);
        expect(secondGet).toBeTruthy();
        expect(secondGet.innerHTML).toEqual(textPrefixedElement);
        expect(len).toEqual(1);
        expect(spy).toHaveBeenCalledTimes(1);
    });

    it('should not find element outside component', async () => {
        const component = getBaseComponent();
        component.elementPrefix = 'prefixed';
        const spy = vi.spyOn(component, 'querySelector');

        const firstGet = component.getElement(keyOutsideElement);
        // run twice to confirm that the second result uses querySelector instead of the cache
        const secondGet = component.getElement(keyOutsideElement);
        const len = Object.keys(component.elementCache).length;

        expect(firstGet).toBeFalsy();
        expect(secondGet).toBeFalsy();
        expect(len).toEqual(0);
        expect(spy).toHaveBeenCalledTimes(2);
    });

    it('should not find element without prefix', async () => {
        const component = getBaseComponent();
        component.elementPrefix = 'prefixed';
        const spy = vi.spyOn(component, 'querySelector');

        const firstGet = component.getElement(keyElement);
        // run twice to confirm that the second result uses querySelector instead of the cache
        const secondGet = component.getElement(keyElement);
        const len = Object.keys(component.elementCache).length;

        expect(firstGet).toBeFalsy();
        expect(secondGet).toBeFalsy();
        expect(len).toEqual(0);
        expect(spy).toHaveBeenCalledTimes(2);
    });
});
