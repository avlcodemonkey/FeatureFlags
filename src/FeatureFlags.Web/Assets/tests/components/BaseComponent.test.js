/**
 * Unit tests for nilla base component.
 */

import assert from 'node:assert/strict';
import { beforeEach, describe, it } from 'node:test';
import isRendered from '../testUtils/isRendered.js';
import setupDom from '../testUtils/setupDom.js';
import tick from '../testUtils/tick.js';

// Setup jsdom first
await setupDom();

// Import the custom element after jsdom is set up
await import('../../js/components/BaseComponent.js');

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

describe('BaseComponent with no prefix', () => {
    beforeEach(async () => {
        document.body.innerHTML = html;
        await isRendered(getBaseComponent);
    });

    it('should query element only once', async () => {
        const component = getBaseComponent();
        let queryCount = 0;
        const originalQuerySelector = component.querySelector;
        component.querySelector = function(selector) {
            queryCount++;
            return originalQuerySelector.call(this, selector);
        };

        const firstGet = component.getElement(keyElement);
        const secondGet = component.getElement(keyElement);
        const len = Object.keys(component.elementCache).length;

        assert.ok(!component.elementPrefix, 'elementPrefix should be falsy');
        assert.ok(firstGet, 'First get should return element');
        assert.strictEqual(firstGet.innerHTML, textElement, 'First get innerHTML should match');
        assert.ok(secondGet, 'Second get should return element');
        assert.strictEqual(secondGet.innerHTML, textElement, 'Second get innerHTML should match');
        assert.strictEqual(len, 1, 'Cache should have one entry');
        assert.strictEqual(queryCount, 1, 'querySelector should be called once');

        component.querySelector = originalQuerySelector;
    });

    it('should not find element outside component', async () => {
        const component = getBaseComponent();
        let queryCount = 0;
        const originalQuerySelector = component.querySelector;
        component.querySelector = function(selector) {
            queryCount++;
            return originalQuerySelector.call(this, selector);
        };

        const firstGet = component.getElement(keyOutsideElement);
        const len = Object.keys(component.elementCache).length;

        assert.ok(!firstGet, 'Should not find element outside component');
        assert.strictEqual(len, 0, 'Cache should be empty');
        assert.strictEqual(queryCount, 1, 'querySelector should be called once');

        component.querySelector = originalQuerySelector;
    });

    it('should not find element with prefix', async () => {
        const component = getBaseComponent();
        let queryCount = 0;
        const originalQuerySelector = component.querySelector;
        component.querySelector = function(selector) {
            queryCount++;
            return originalQuerySelector.call(this, selector);
        };

        const firstGet = component.getElement(keyPrefixedElement);
        const len = Object.keys(component.elementCache).length;

        assert.ok(!firstGet, 'Should not find element with prefix');
        assert.strictEqual(len, 0, 'Cache should be empty');
        assert.strictEqual(queryCount, 1, 'querySelector should be called once');

        component.querySelector = originalQuerySelector;
    });

    it('should empty cache when removed from dom', async () => {
        const component = getBaseComponent();
        let queryCount = 0;
        const originalQuerySelector = component.querySelector;
        component.querySelector = function(selector) {
            queryCount++;
            return originalQuerySelector.call(this, selector);
        };

        const firstGet = component.getElement(keyElement);
        const firstLen = Object.keys(component.elementCache).length;
        const secondGet = component.getElement(`prefixed-${keyPrefixedElement}`);
        const secondLen = Object.keys(component.elementCache).length;

        // remove the component from the dom and trigger the disconnectedCallback
        document.body.innerHTML = '';
        await tick();
        const thirdLen = Object.keys(component.elementCache).length;

        assert.ok(firstGet, 'First get should return element');
        assert.strictEqual(firstLen, 1, 'Cache should have one entry after first get');
        assert.ok(secondGet, 'Second get should return element');
        assert.strictEqual(secondLen, 2, 'Cache should have two entries after second get');
        assert.strictEqual(queryCount, 2, 'querySelector should be called twice');
        assert.strictEqual(thirdLen, 0, 'Cache should be empty after removal');

        component.querySelector = originalQuerySelector;
    });
});

describe('BaseComponent with prefix', () => {
    beforeEach(async () => {
        document.body.innerHTML = html;
        await isRendered(getBaseComponent);
    });

    it('should query element only once', async () => {
        const component = getBaseComponent();
        component.elementPrefix = 'prefixed';
        let queryCount = 0;
        const originalQuerySelector = component.querySelector;
        component.querySelector = function(selector) {
            queryCount++;
            return originalQuerySelector.call(this, selector);
        };

        const firstGet = component.getElement(keyPrefixedElement);
        const secondGet = component.getElement(keyPrefixedElement);
        const len = Object.keys(component.elementCache).length;

        assert.ok(firstGet, 'First get should return element');
        assert.strictEqual(firstGet.innerHTML, textPrefixedElement, 'First get innerHTML should match');
        assert.ok(secondGet, 'Second get should return element');
        assert.strictEqual(secondGet.innerHTML, textPrefixedElement, 'Second get innerHTML should match');
        assert.strictEqual(len, 1, 'Cache should have one entry');
        assert.strictEqual(queryCount, 1, 'querySelector should be called once');

        component.querySelector = originalQuerySelector;
    });

    it('should not find element outside component', async () => {
        const component = getBaseComponent();
        component.elementPrefix = 'prefixed';
        let queryCount = 0;
        const originalQuerySelector = component.querySelector;
        component.querySelector = function(selector) {
            queryCount++;
            return originalQuerySelector.call(this, selector);
        };

        const firstGet = component.getElement(keyOutsideElement);
        const secondGet = component.getElement(keyOutsideElement);
        const len = Object.keys(component.elementCache).length;

        assert.ok(!firstGet, 'Should not find element outside component');
        assert.ok(!secondGet, 'Should not find element outside component');
        assert.strictEqual(len, 0, 'Cache should be empty');
        assert.strictEqual(queryCount, 2, 'querySelector should be called twice');

        component.querySelector = originalQuerySelector;
    });

    it('should not find element without prefix', async () => {
        const component = getBaseComponent();
        component.elementPrefix = 'prefixed';
        let queryCount = 0;
        const originalQuerySelector = component.querySelector;
        component.querySelector = function(selector) {
            queryCount++;
            return originalQuerySelector.call(this, selector);
        };

        const firstGet = component.getElement(keyElement);
        const secondGet = component.getElement(keyElement);
        const len = Object.keys(component.elementCache).length;

        assert.ok(!firstGet, 'Should not find element without prefix');
        assert.ok(!secondGet, 'Should not find element without prefix');
        assert.strictEqual(len, 0, 'Cache should be empty');
        assert.strictEqual(queryCount, 2, 'querySelector should be called twice');

        component.querySelector = originalQuerySelector;
    });
});
