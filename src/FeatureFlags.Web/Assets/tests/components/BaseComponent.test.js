/**
 * Unit tests for nilla base component.
 */

import assert from 'node:assert/strict';
import { beforeEach, describe, it } from 'node:test';
import isRendered from '../testUtils/isRendered.js';
import setupDom from '../testUtils/setupDom.js';

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
    <div>
        <nilla-base>
            <div data-${keyElement}>${textElement}</div>
            <div data-prefixed-${keyPrefixedElement}>${textPrefixedElement}</div>
        </nilla-base>
        <div data-${keyOutsideElement}>${textOutsideElement}</div>
    </div>
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
        const originalQuerySelectorAll = component.querySelectorAll;
        component.querySelectorAll = function(selector) {
            queryCount++;
            return originalQuerySelectorAll.call(this, selector);
        };

        const firstGet = component.getElement(keyElement);
        const secondGet = component.getElement(keyElement);

        assert.ok(!component.elementPrefix, 'elementPrefix should be falsy');
        assert.ok(firstGet, 'First get should return element');
        assert.strictEqual(firstGet.innerHTML, textElement, 'First get innerHTML should match');
        assert.ok(secondGet, 'Second get should return element');
        assert.strictEqual(secondGet.innerHTML, textElement, 'Second get innerHTML should match');
        assert.strictEqual(queryCount, 1, 'querySelectorAll should be called once');

        component.querySelectorAll = originalQuerySelectorAll;
    });

    it('should not find element outside component', async () => {
        const component = getBaseComponent();
        let queryCount = 0;
        const originalQuerySelectorAll = component.querySelectorAll;
        component.querySelectorAll = function(selector) {
            queryCount++;
            return originalQuerySelectorAll.call(this, selector);
        };

        const firstGet = component.getElement(keyOutsideElement);

        assert.ok(!firstGet, 'Should not find element outside component');
        assert.strictEqual(queryCount, 1, 'querySelectorAll should be called once');

        component.querySelectorAll = originalQuerySelectorAll;
    });

    it('should not find element with prefix', async () => {
        const component = getBaseComponent();
        let queryCount = 0;
        const originalQuerySelectorAll = component.querySelectorAll;
        component.querySelectorAll = function(selector) {
            queryCount++;
            return originalQuerySelectorAll.call(this, selector);
        };

        const firstGet = component.getElement(keyPrefixedElement);

        assert.ok(!firstGet, 'Should not find element with prefix');
        assert.strictEqual(queryCount, 1, 'querySelectorAll should be called once');

        component.querySelectorAll = originalQuerySelectorAll;
    });
});

describe('BaseComponent with prefix', () => {
    beforeEach(async () => {
        document.body.innerHTML = html;
        await isRendered(getBaseComponent);
        getBaseComponent()?._setKey('prefixed');
    });

    it('should query element only once', async () => {
        const component = getBaseComponent();
        let queryCount = 0;
        const originalQuerySelectorAll = component.querySelectorAll;
        component.querySelectorAll = function(selector) {
            queryCount++;
            return originalQuerySelectorAll.call(this, selector);
        };

        const firstGet = component.getElement(keyPrefixedElement);
        const secondGet = component.getElement(keyPrefixedElement);

        assert.ok(firstGet, 'First get should return element');
        assert.strictEqual(firstGet.innerHTML, textPrefixedElement, 'First get innerHTML should match');
        assert.ok(secondGet, 'Second get should return element');
        assert.strictEqual(secondGet.innerHTML, textPrefixedElement, 'Second get innerHTML should match');
        assert.strictEqual(queryCount, 1, 'querySelectorAll should be called once');

        component.querySelectorAll = originalQuerySelectorAll;
    });

    it('should not find element outside component', async () => {
        const component = getBaseComponent();
        let queryCount = 0;
        const originalQuerySelectorAll = component.querySelectorAll;
        component.querySelectorAll = function(selector) {
            queryCount++;
            return originalQuerySelectorAll.call(this, selector);
        };

        const firstGet = component.getElement(keyOutsideElement);
        const secondGet = component.getElement(keyOutsideElement);

        assert.ok(!firstGet, 'Should not find element outside component');
        assert.ok(!secondGet, 'Should not find element outside component');
        assert.strictEqual(queryCount, 2, 'querySelectorAll should be called twice');

        component.querySelectorAll = originalQuerySelectorAll;
    });

    it('should not find element without prefix', async () => {
        const component = getBaseComponent();
        let queryCount = 0;
        const originalQuerySelectorAll = component.querySelectorAll;
        component.querySelectorAll = function(selector) {
            queryCount++;
            return originalQuerySelectorAll.call(this, selector);
        };

        const firstGet = component.getElement(keyElement);
        const secondGet = component.getElement(keyElement);

        assert.ok(!firstGet, 'Should not find element without prefix');
        assert.ok(!secondGet, 'Should not find element without prefix');
        assert.strictEqual(queryCount, 2, 'querySelectorAll should be called twice');

        component.querySelectorAll = originalQuerySelectorAll;
    });
});

describe('BaseComponent parentNode logic', () => {
    beforeEach(async () => {
        // Simulate nested nilla-base components
        document.body.innerHTML = `
            <nilla-base id="outer">
                <nilla-base id="inner">
                    <div data-nested-key>nested value</div>
                </nilla-base>
            </nilla-base>
        `;
        await isRendered(() => document.getElementById('outer'));
        await isRendered(() => document.getElementById('inner'));
    });

    it('should only cache element if it belongs to the correct component (inner)', () => {
        const inner = document.getElementById('inner');
        const outer = document.getElementById('outer');
        // The inner component should find its own element
        const el = inner.getElement('nested-key');
        assert.ok(el, 'Inner component should find its nested element');
        assert.strictEqual(el.innerHTML, 'nested value', 'Inner element value should match');
        // The outer component should NOT find the inner element
        const elOuter = outer.getElement('nested-key');
        assert.ok(!elOuter, 'Outer component should not find inner component\'s element');
    });

    it('should not cache element if parentNode does not match component', () => {
        // Add an element outside any nilla-base
        const orphanDiv = document.createElement('div');
        orphanDiv.setAttribute('data-orphan-key', 'true');
        orphanDiv.innerHTML = 'orphan value';
        document.body.appendChild(orphanDiv);

        const inner = document.getElementById('inner');
        const el = inner.getElement('orphan-key');
        assert.ok(!el, 'Should not find element outside component');
    });

    it('should handle deeply nested elements', () => {
        // Add a deeply nested element inside inner
        const inner = document.getElementById('inner');
        const deepDiv = document.createElement('div');
        deepDiv.setAttribute('data-deep-key', 'true');
        deepDiv.innerHTML = 'deep value';
        const wrapper = document.createElement('div');
        wrapper.appendChild(deepDiv);
        inner.appendChild(wrapper);

        const el = inner.getElement('deep-key');
        assert.ok(el, 'Should find deeply nested element');
        assert.strictEqual(el.innerHTML, 'deep value', 'Deep element value should match');
    });
});
