/**
 * Unit tests for nilla-pjax.
 */

import assert from 'node:assert/strict';
import { beforeEach, describe, it } from 'node:test';
import isRendered from '../testUtils/isRendered.js';
import setupDom from '../testUtils/setupDom.js';

// Setup jsdom first
await setupDom();

// Import the custom element after jsdom is set up
await import('../../js/components/PJax.js');

const pjaxHtml = `
    <nilla-pjax data-version="1.2.3">
        <div data-pjax-target>Initial Content</div>
        <div data-pjax-loading-indicator></div>
        <nilla-info data-pjax-info-dialog></nilla-info>
        <a href="/test" id="test-link">Test Link</a>
        <form action="/form" method="post" id="test-form">
            <input name="field" value="value" />
            <button type="submit">Submit</button>
        </form>
    </nilla-pjax>
`;

/**
 * Gets the pjax element.
 * @returns {HTMLElement | null | undefined} PJax element
 */
function getPjax() {
    return document.body.querySelector('nilla-pjax');
}

/**
 * Gets the target element for content replacement.
 * @returns {HTMLElement | null | undefined} Target element
 */
// function getTarget() {
//     return getPjax()?.querySelector('[data-pjax-target]');
// }

/**
 * Gets the loading indicator element.
 * @returns {HTMLElement | null | undefined} Loading indicator element
 */
// function getLoadingIndicator() {
//     return getPjax()?.querySelector('[data-pjax-loading-indicator]');
// }

/**
 * Gets the info dialog element.
 * @returns {HTMLElement | null | undefined} Info dialog element
 */
// function getInfoDialog() {
//     return getPjax()?.querySelector('[data-pjax-info-dialog]');
// }

describe('nilla-pjax', () => {
    beforeEach(async () => {
        document.body.innerHTML = pjaxHtml;
        await isRendered(getPjax);
    });

    it('should add popstate listener on init', async () => {
        let popStateAdded = false;
        const originalAddEventListener = window.addEventListener;
        window.addEventListener = (type, fn) => {
            if (type === 'popstate') popStateAdded = true;
            // Call through to original to avoid breaking other logic
            return originalAddEventListener.call(window, type, fn);
        };

        // Re-render PJax to trigger initialization
        document.body.innerHTML = pjaxHtml;
        await isRendered(getPjax);

        assert.ok(popStateAdded, 'popstate listener should be added');
        window.addEventListener = originalAddEventListener;
    });

    it('should add click listener on component on init', async () => {
        let clickAdded = false;
        // Patch addEventListener on the prototype before element is created
        const originalAddEventListener = HTMLElement.prototype.addEventListener;
        HTMLElement.prototype.addEventListener = function(type, fn) {
            if (type === 'click' && this.tagName === 'NILLA-PJAX') {
                clickAdded = true;
            }
            return originalAddEventListener.call(this, type, fn);
        };

        // Re-render PJax to trigger initialization
        document.body.innerHTML = pjaxHtml;
        await isRendered(getPjax);

        assert.ok(clickAdded, 'click listener should be added on PJax component');
        HTMLElement.prototype.addEventListener = originalAddEventListener;
    });

    it('should add submit listener on component on init', async () => {
        let submitAdded = false;
        const originalAddEventListener = HTMLElement.prototype.addEventListener;
        HTMLElement.prototype.addEventListener = function(type, fn) {
            if (type === 'submit' && this.tagName === 'NILLA-PJAX') {
                submitAdded = true;
            }
            return originalAddEventListener.call(this, type, fn);
        };

        // Re-render PJax to trigger initialization
        document.body.innerHTML = pjaxHtml;
        await isRendered(getPjax);

        assert.ok(submitAdded, 'submit listener should be added on PJax component');
        HTMLElement.prototype.addEventListener = originalAddEventListener;
    });

    it('should replace state on init', async () => {
        let stateReplaced = false;
        const originalReplaceState = window.history.replaceState;
        window.history.replaceState = function(state, title, url) {
            stateReplaced = true;
            return originalReplaceState.call(this, state, title, url);
        };

        // Re-render PJax to trigger initialization
        document.body.innerHTML = pjaxHtml;
        await isRendered(getPjax);

        assert.ok(stateReplaced, 'history.replaceState should be called on init');
        window.history.replaceState = originalReplaceState;
    });

    it('should ', async () => {
    });

    it('should remove popstate listener on disconnect', async () => {
        const pjax = getPjax();
        let popStateRemoved = false;
        const originalRemoveEventListener = window.removeEventListener;
        // eslint-disable-next-line no-unused-vars
        window.removeEventListener = (type, fn) => {
            if (type === 'popstate') popStateRemoved = true;
        };
        // Remove PJax from DOM
        pjax.remove();

        // Wait for disconnectedCallback
        await new Promise(resolve => setTimeout(resolve, 10));

        assert.ok(popStateRemoved, 'popstate listener should be removed');
        window.removeEventListener = originalRemoveEventListener;
    });

    it('should remove click listener on component on disconnect', async () => {
        const pjax = getPjax();
        let clickRemoved = false;
        const originalRemoveEventListener = HTMLElement.prototype.removeEventListener;
        HTMLElement.prototype.removeEventListener = function(type, fn) {
            if (type === 'click' && this === pjax) {
                clickRemoved = true;
            }
            return originalRemoveEventListener.call(this, type, fn);
        };

        // Remove PJax from DOM to trigger disconnectedCallback
        pjax.remove();
        // Wait for disconnectedCallback
        await new Promise(resolve => setTimeout(resolve, 10));

        assert.ok(clickRemoved, 'click listener should be removed from PJax component');
        HTMLElement.prototype.removeEventListener = originalRemoveEventListener;
    });

    it('should remove submit listener on component on disconnect', async () => {
        const pjax = getPjax();
        let submitRemoved = false;
        const originalRemoveEventListener = HTMLElement.prototype.removeEventListener;
        HTMLElement.prototype.removeEventListener = function(type, fn) {
            if (type === 'submit' && this === pjax) {
                submitRemoved = true;
            }
            return originalRemoveEventListener.call(this, type, fn);
        };

        // Remove PJax from DOM to trigger disconnectedCallback
        pjax.remove();
        // Wait for disconnectedCallback
        await new Promise(resolve => setTimeout(resolve, 10));

        assert.ok(submitRemoved, 'submit listener should be removed from PJax component');
        HTMLElement.prototype.removeEventListener = originalRemoveEventListener;
    });
});
