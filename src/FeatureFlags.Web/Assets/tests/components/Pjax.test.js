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
function getTarget() {
    return getPjax()?.querySelector('[data-pjax-target]');
}

/**
 * Gets the loading indicator element.
 * @returns {HTMLElement | null | undefined} Loading indicator element
 */
function getLoadingIndicator() {
    return getPjax()?.querySelector('[data-pjax-loading-indicator]');
}

/**
 * Gets the info dialog element.
 * @returns {HTMLElement | null | undefined} Info dialog element
 */
function getInfoDialog() {
    return getPjax()?.querySelector('[data-pjax-info-dialog]');
}

describe('nilla-pjax', () => {
    beforeEach(async () => {
        document.body.innerHTML = pjaxHtml;
        await isRendered(getPjax);
    });

    it('should initialize with version from dataset', async () => {
        const pjax = getPjax();

        assert.strictEqual(pjax.version, '1.2.3', 'Version should be set from dataset');
    });

    it('should add loading indicator class on showLoadingIndicator', async () => {
        const pjax = getPjax();
        const indicator = getLoadingIndicator();
        assert.ok(indicator, 'Loading indicator should exist');
        pjax.showLoadingIndicator();

        assert.ok(indicator.classList.contains('pjax-request'), 'Loading indicator should have pjax-request class');
    });

    it('should remove loading indicator class on hideLoadingIndicator', async () => {
        const pjax = getPjax();
        const indicator = getLoadingIndicator();
        indicator.classList.add('pjax-request');
        pjax.hideLoadingIndicator();

        assert.ok(!indicator.classList.contains('pjax-request'), 'Loading indicator should not have pjax-request class');
    });

    it('should show error dialog on handleResponseError', async () => {
        const pjax = getPjax();
        const infoDialog = getInfoDialog();
        let showCalled = false;
        infoDialog.show = () => {
            showCalled = true;
        };
        await pjax.handleResponseError(new Error('Test error'));

        assert.ok(showCalled, 'Info dialog show should be called on error');
    });

    it('should update target content on processResponse with HTML', async () => {
        const pjax = getPjax();
        const target = getTarget();

        const response = new Response();
        response.headers.set('content-type', 'text/html');
        response.text = async () => '<span>New Content</span>';

        await pjax.processResponse(new URL('/test', 'http://localhost'), false, response);

        assert.ok(target.innerHTML.includes('New Content'), 'Target content should be updated');
    });

    it('should not update target content if response is JSON', async () => {
        const pjax = getPjax();
        const target = getTarget();
        target.innerHTML = 'Initial Content';

        const response = new Response();
        response.headers.set('content-type', 'application/json');
        response.json = async () => '{}';

        await pjax.processResponse(new URL('/test', 'http://localhost'), true, response);

        assert.strictEqual(target.innerHTML, 'Initial Content', 'Target content should not be updated for JSON');
        delete global.isJson;
    });

    // @TODO finish implementing pjax tests.

    it('should clean up event listeners on disconnectedCallback', async () => {
        const pjax = getPjax();

        // Patch removeEventListener to track calls
        let popStateRemoved = false, clickRemoved = false, submitRemoved = false;
        const originalRemoveEventListener = window.removeEventListener;
        // eslint-disable-next-line no-unused-vars
        window.removeEventListener = (type, fn) => {
            if (type === 'popstate') popStateRemoved = true;
        };

        const originalPjaxRemoveEventListener = pjax.removeEventListener;
        // eslint-disable-next-line no-unused-vars
        pjax.removeEventListener = (type, fn) => {
            if (type === 'click') clickRemoved = true;
            if (type === 'submit') submitRemoved = true;
        };
        pjax.disconnectedCallback();

        assert.ok(popStateRemoved, 'popstate listener should be removed');
        assert.ok(clickRemoved, 'click listener should be removed');
        assert.ok(submitRemoved, 'submit listener should be removed');

        // Restore original listeners
        window.removeEventListener = originalRemoveEventListener;
        pjax.removeEventListener = originalPjaxRemoveEventListener;
    });
});
