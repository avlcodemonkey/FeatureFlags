/**
 * Unit tests for nilla-pjax.
 */

import assert from 'node:assert/strict';
import { beforeEach, describe, it } from 'node:test';
import isRendered from '../testUtils/isRendered.js';
import setupDom from '../testUtils/setupDom.js';

const defaultTitle = 'Pjax Test';
const defaultUrl = 'https://localhost/';
// Setup jsdom first
await setupDom(`<!DOCTYPE html><html><head><title>${defaultTitle}</title></head><body></body></html>`, defaultUrl);

// Import the custom element after jsdom is set up
await import('../../js/components/PJax.js');

const pjaxHtml = `
    <nilla-pjax data-version="1.2.3">
        <div data-pjax-target>Initial Content</div>
        <div data-pjax-loading-indicator></div>
        <nilla-info data-pjax-info-dialog></nilla-info>
        <a href="https://localhost/test-link" id="test-link">Test Link</a>
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
        let stateTitle = '';
        let stateUrl = '';
        window.history.replaceState = function(state, unused) {
            stateTitle = state.title;
            stateUrl = state.url;
            stateReplaced = true;
            return originalReplaceState.call(this, state, unused);
        };

        // Re-render PJax to trigger initialization
        document.body.innerHTML = pjaxHtml;
        await isRendered(getPjax);

        assert.ok(stateReplaced, 'history.replaceState should be called on init');
        assert.strictEqual(stateTitle, defaultTitle, 'history.replaceState should be called with current document title');
        assert.strictEqual(stateUrl, defaultUrl, 'history.replaceState should be called with current URL');
        window.history.replaceState = originalReplaceState;
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

    it('should request a page and update content after onPopState', async () => {
        // Arrange: set up initial state and spy on fetch
        const target = getTarget();
        target.innerHTML = 'Initial Content';

        // Patch PJax's fetch to simulate a navigation response
        const originalFetch = global.fetch;
        global.fetch = async () => {
            return new Response('<span>PopState Content</span>', {
                status: 200,
                headers: { 'Content-Type': 'text/html', 'x-pjax-title': 'New Title' },
            });
        };

        // Act: simulate popstate event
        window.dispatchEvent(new PopStateEvent('popstate', { state: { url: 'https://localhost' } }));

        // Wait for PJax to process the event and update the DOM
        await new Promise(resolve => setTimeout(resolve, 50));

        // Assert: target content should be updated
        assert.ok(
            target.innerHTML.includes('PopState Content'),
            'Target content should be updated after popstate navigation',
        );
        assert.strictEqual(document.title, 'New Title', 'Document title should be updated after popstate navigation');

        // Cleanup
        global.fetch = originalFetch;
    });

    it('should not request a page if popstate event has no state', async () => {
        const target = getTarget();
        target.innerHTML = 'Initial Content';

        // Patch fetch to fail if called
        const originalFetch = global.fetch;
        let fetchCalled = false;
        global.fetch = async () => {
            fetchCalled = true;
            return new Response('<span>Should not be called</span>', { status: 200 });
        };

        // Act: simulate popstate event with no state
        window.dispatchEvent(new PopStateEvent('popstate', { state: null }));

        // Wait for PJax to process
        await new Promise(resolve => setTimeout(resolve, 30));

        // Assert: fetch should not be called, content should not change
        assert.strictEqual(target.innerHTML, 'Initial Content', 'Content should not change');
        assert.ok(!fetchCalled, 'fetch should not be called');

        global.fetch = originalFetch;
    });

    it('should not request a page if popstate event state has no url', async () => {
        const target = getTarget();
        target.innerHTML = 'Initial Content';

        // Patch fetch to fail if called
        const originalFetch = global.fetch;
        let fetchCalled = false;
        global.fetch = async () => {
            fetchCalled = true;
            return new Response('<span>Should not be called</span>', { status: 200 });
        };

        // Act: simulate popstate event with state missing url
        window.dispatchEvent(new PopStateEvent('popstate', { state: { title: 'Missing URL' } }));

        // Wait for PJax to process
        await new Promise(resolve => setTimeout(resolve, 30));

        // Assert: fetch should not be called, content should not change
        assert.strictEqual(target.innerHTML, 'Initial Content', 'Content should not change');
        assert.ok(!fetchCalled, 'fetch should not be called');

        global.fetch = originalFetch;
    });

    it('should not request a page if popstate event state url is invalid', async () => {
        const target = getTarget();
        target.innerHTML = 'Initial Content';

        // Patch fetch to fail if called
        const originalFetch = global.fetch;
        let fetchCalled = false;
        global.fetch = async () => {
            fetchCalled = true;
            return new Response('<span>Should not be called</span>', { status: 200 });
        };

        // Act: simulate popstate event with invalid url
        window.dispatchEvent(new PopStateEvent('popstate', { state: { url: 'https://[invalid-url]' } }));

        // Wait for PJax to process
        await new Promise(resolve => setTimeout(resolve, 30));

        // Assert: fetch should not be called, content should not change
        assert.strictEqual(target.innerHTML, 'Initial Content', 'Content should not change');
        assert.ok(!fetchCalled, 'fetch should not be called');

        global.fetch = originalFetch;
    });

    it('should show error dialog if fetch fails during onPopState', async () => {
        // Arrange: mock fetch to throw an error
        const originalFetch = global.fetch;
        global.fetch = async () => {
            throw new Error('Simulated fetch error');
        };

        // Spy on info dialog show method
        const pjax = getPjax();
        const infoDialog = pjax.querySelector('[data-pjax-info-dialog]');
        let showCalled = false;
        infoDialog.show = () => {
            showCalled = true;
        };

        // Act: simulate popstate event with new url
        window.dispatchEvent(new PopStateEvent('popstate', { state: { url: 'https://localhost/abc' } }));
        // Wait for PJax to process
        await new Promise(resolve => setTimeout(resolve, 50));

        // Assert: info dialog should be shown
        assert.ok(showCalled, 'Info dialog show should be called on fetch error');

        // Cleanup
        global.fetch = originalFetch;
    });

    it('should request a page and update content after onClickListener', async () => {
        const target = getTarget();
        const indicator = getLoadingIndicator();
        target.innerHTML = 'Initial Content';

        // Patch fetch to simulate a navigation response
        const originalFetch = global.fetch;
        global.fetch = async () => {
            // Assert loading indicator is shown during fetch
            assert.ok(indicator.classList.contains('pjax-request'), 'Loading indicator should be visible during fetch');
            return new Response('<span>Clicked Content</span>', {
                status: 200,
                headers: { 'Content-Type': 'text/html', 'x-pjax-title': 'Clicked Title' },
            });
        };

        // Act: simulate click event on valid internal link
        const link = document.getElementById('test-link');
        const event = new MouseEvent('click', { bubbles: true, cancelable: true });
        event.preventDefault(); // prevent jsdom from trying to navigate since it doesn't support it
        link.dispatchEvent(event);

        await new Promise(resolve => setTimeout(resolve, 50));

        assert.ok(
            target.innerHTML.includes('Clicked Content'),
            'Target content should be updated after link click',
        );
        assert.strictEqual(document.title, 'Clicked Title', 'Document title should be updated after link click');

        global.fetch = originalFetch;
    });

    it('should not request a page if link has exclude attribute', async () => {
        const target = getTarget();
        target.innerHTML = 'Initial Content';

        // Add exclude attribute to link
        const link = document.getElementById('test-link');
        link.setAttribute('data-pjax-no-follow', '');

        // Patch fetch to fail if called
        const originalFetch = global.fetch;
        let fetchCalled = false;
        global.fetch = async () => {
            fetchCalled = true;
            return new Response('<span>Should not be called</span>', { status: 200 });
        };

        const event = new MouseEvent('click', { bubbles: true, cancelable: true });
        event.preventDefault(); // prevent jsdom from trying to navigate since it doesn't support it
        link.dispatchEvent(event);
        await new Promise(resolve => setTimeout(resolve, 30));

        assert.strictEqual(target.innerHTML, 'Initial Content', 'Content should not change');
        assert.ok(!fetchCalled, 'fetch should not be called');

        global.fetch = originalFetch;
    });

    it('should not request a page if link is external', async () => {
        const target = getTarget();
        target.innerHTML = 'Initial Content';

        // Change link to external
        const link = document.getElementById('test-link');
        link.href = 'https://external.com/test';

        // Patch fetch to fail if called
        const originalFetch = global.fetch;
        let fetchCalled = false;
        global.fetch = async () => {
            fetchCalled = true;
            return new Response('<span>Should not be called</span>', { status: 200 });
        };

        const event = new MouseEvent('click', { bubbles: true, cancelable: true });
        event.preventDefault(); // prevent jsdom from trying to navigate since it doesn't support it
        link.dispatchEvent(event);
        await new Promise(resolve => setTimeout(resolve, 30));

        assert.strictEqual(target.innerHTML, 'Initial Content', 'Content should not change');
        assert.ok(!fetchCalled, 'fetch should not be called');

        global.fetch = originalFetch;
    });

    it('should not request a page if link is to an ignored file type', async () => {
        const target = getTarget();
        target.innerHTML = 'Initial Content';

        // Change link to a PDF file
        const link = document.getElementById('test-link');
        link.href = 'https://localhost/file.pdf';

        // Patch fetch to fail if called
        const originalFetch = global.fetch;
        let fetchCalled = false;
        global.fetch = async () => {
            fetchCalled = true;
            return new Response('<span>Should not be called</span>', { status: 200 });
        };

        const event = new MouseEvent('click', { bubbles: true, cancelable: true });
        event.preventDefault(); // prevent jsdom from trying to navigate since it doesn't support it
        link.dispatchEvent(event);
        await new Promise(resolve => setTimeout(resolve, 30));

        assert.strictEqual(target.innerHTML, 'Initial Content', 'Content should not change');
        assert.ok(!fetchCalled, 'fetch should not be called');

        global.fetch = originalFetch;
    });

    it('should not request a page if link has invalid URL', async () => {
        const target = getTarget();
        target.innerHTML = 'Initial Content';

        // Change link to an invalid URL
        const link = document.getElementById('test-link');
        link.href = 'https://[invalid-url]';

        // Patch fetch to fail if called
        const originalFetch = global.fetch;
        let fetchCalled = false;
        global.fetch = async () => {
            fetchCalled = true;
            return new Response('<span>Should not be called</span>', { status: 200 });
        };

        link.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true }));
        await new Promise(resolve => setTimeout(resolve, 30));

        assert.strictEqual(target.innerHTML, 'Initial Content', 'Content should not change');
        assert.ok(!fetchCalled, 'fetch should not be called');

        global.fetch = originalFetch;
    });

    it('should not request a page if clicked element is not a link', async () => {
        const target = getTarget();
        target.innerHTML = 'Initial Content';

        // Patch fetch to fail if called
        const originalFetch = global.fetch;
        let fetchCalled = false;
        global.fetch = async () => {
            fetchCalled = true;
            return new Response('<span>Should not be called</span>', { status: 200 });
        };

        // Act: simulate click event on a div
        target.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true }));
        await new Promise(resolve => setTimeout(resolve, 30));

        assert.strictEqual(target.innerHTML, 'Initial Content', 'Content should not change');
        assert.ok(!fetchCalled, 'fetch should not be called');

        global.fetch = originalFetch;
    });

    it('should show error dialog if fetch fails during onClickListener', async () => {
        const indicator = getLoadingIndicator();
        // Arrange: mock fetch to throw an error
        const originalFetch = global.fetch;
        global.fetch = async () => {
            assert.ok(indicator.classList.contains('pjax-request'), 'Loading indicator should be visible during fetch');
            throw new Error('Simulated fetch error');
        };
        const link = document.getElementById('test-link');
        link.href = 'https://localhost/new-link';

        // Spy on info dialog show method
        const infoDialog = getInfoDialog();
        let showCalled = false;
        infoDialog.show = () => {
            showCalled = true;
        };

        // Act: simulate link click to trigger fetch
        const event = new MouseEvent('click', { bubbles: true, cancelable: true });
        Object.defineProperty(event, 'target', { value: link, enumerable: true });
        link.dispatchEvent(event);

        // Wait for PJax to process
        await new Promise(resolve => setTimeout(resolve, 50));

        // Assert: info dialog should be shown and loading indicator hidden
        assert.ok(showCalled, 'Info dialog show should be called on fetch error');
        assert.ok(!indicator.classList.contains('pjax-request'), 'Loading indicator should be hidden after fetch error');

        global.fetch = originalFetch;
    });

    it('should request a page and update content after onSubmitListener', async () => {
        const target = getTarget();
        const indicator = getLoadingIndicator();
        target.innerHTML = 'Initial Content';

        // Patch fetch to simulate a form submission response
        const originalFetch = global.fetch;
        global.fetch = async () => {
            assert.ok(indicator.classList.contains('pjax-request'), 'Loading indicator should be visible during fetch');
            return new Response('<span>Form Submitted Content</span>', {
                status: 200,
                headers: { 'Content-Type': 'text/html', 'x-pjax-title': 'Form Title' },
            });
        };

        // Act: simulate submit event on valid form
        const form = document.getElementById('test-form');
        const event = new window.Event('submit', { bubbles: true, cancelable: true });
        event.preventDefault(); // prevent jsdom from trying to submit
        form.dispatchEvent(event);

        await new Promise(resolve => setTimeout(resolve, 50));

        assert.ok(
            target.innerHTML.includes('Form Submitted Content'),
            'Target content should be updated after form submit',
        );
        assert.strictEqual(document.title, 'Form Title', 'Document title should be updated after form submit');
        assert.ok(!indicator.classList.contains('pjax-request'), 'Loading indicator should be hidden after fetch');

        global.fetch = originalFetch;
    });

    it('should not request a page if form has exclude attribute', async () => {
        const target = getTarget();
        target.innerHTML = 'Initial Content';

        // Add exclude attribute to form
        const form = document.getElementById('test-form');
        form.setAttribute('data-pjax-no-follow', '');

        // Patch fetch to fail if called
        const originalFetch = global.fetch;
        let fetchCalled = false;
        global.fetch = async () => {
            fetchCalled = true;
            return new Response('<span>Should not be called</span>', { status: 200 });
        };

        const event = new window.Event('submit', { bubbles: true, cancelable: true });
        event.preventDefault();
        form.dispatchEvent(event);
        await new Promise(resolve => setTimeout(resolve, 30));

        assert.strictEqual(target.innerHTML, 'Initial Content', 'Content should not change');
        assert.ok(!fetchCalled, 'fetch should not be called');

        global.fetch = originalFetch;
    });

    it('should not request a page if form action is invalid', async () => {
        const target = getTarget();
        target.innerHTML = 'Initial Content';

        // Set form action to invalid URL
        const form = document.getElementById('test-form');
        form.action = 'https://[invalid-url]';

        // Patch fetch to fail if called
        const originalFetch = global.fetch;
        let fetchCalled = false;
        global.fetch = async () => {
            fetchCalled = true;
            return new Response('<span>Should not be called</span>', { status: 200 });
        };

        const event = new window.Event('submit', { bubbles: true, cancelable: true });
        event.preventDefault();
        form.dispatchEvent(event);
        await new Promise(resolve => setTimeout(resolve, 30));

        assert.strictEqual(target.innerHTML, 'Initial Content', 'Content should not change');
        assert.ok(!fetchCalled, 'fetch should not be called');

        global.fetch = originalFetch;
    });

    it('should not request a page if form action is external', async () => {
        const target = getTarget();
        target.innerHTML = 'Initial Content';

        // Set form action to external origin
        const form = document.getElementById('test-form');
        form.action = 'https://external.com/form';

        // Patch fetch to fail if called
        const originalFetch = global.fetch;
        let fetchCalled = false;
        global.fetch = async () => {
            fetchCalled = true;
            return new Response('<span>Should not be called</span>', { status: 200 });
        };

        const event = new window.Event('submit', { bubbles: true, cancelable: true });
        event.preventDefault();
        form.dispatchEvent(event);
        await new Promise(resolve => setTimeout(resolve, 30));

        assert.strictEqual(target.innerHTML, 'Initial Content', 'Content should not change');
        assert.ok(!fetchCalled, 'fetch should not be called');

        global.fetch = originalFetch;
    });

    it('should show error dialog if fetch fails during onSubmitListener', async () => {
        // Arrange: mock fetch to throw an error
        const originalFetch = global.fetch;
        global.fetch = async () => {
            throw new Error('Simulated fetch error');
        };

        // Spy on info dialog show method
        const infoDialog = getInfoDialog();
        let showCalled = false;
        infoDialog.show = () => {
            showCalled = true;
        };

        // Act: simulate submit event on valid form
        const form = document.getElementById('test-form');
        const event = new window.Event('submit', { bubbles: true, cancelable: true });
        event.preventDefault();
        form.dispatchEvent(event);

        // Wait for PJax to process
        await new Promise(resolve => setTimeout(resolve, 50));

        // Assert: info dialog should be shown
        assert.ok(showCalled, 'Info dialog show should be called on fetch error');

        global.fetch = originalFetch;
    });

    it('should not request a page if submitted element is not a form', async () => {
        const target = getTarget();
        target.innerHTML = 'Initial Content';

        // Patch fetch to fail if called
        const originalFetch = global.fetch;
        let fetchCalled = false;
        global.fetch = async () => {
            fetchCalled = true;
            return new Response('<span>Should not be called</span>', { status: 200 });
        };

        // Act: simulate submit event on a div
        target.dispatchEvent(new window.Event('submit', { bubbles: true, cancelable: true }));
        await new Promise(resolve => setTimeout(resolve, 30));

        assert.strictEqual(target.innerHTML, 'Initial Content', 'Content should not change');
        assert.ok(!fetchCalled, 'fetch should not be called');

        global.fetch = originalFetch;
    });
});
