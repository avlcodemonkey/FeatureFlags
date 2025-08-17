import DefaultTimeout from '../constants/Fetch.js';
import HttpHeaders from '../constants/HttpHeaders.js';
import HttpMethods from '../constants/HttpMethods.js';
import FetchError from '../errors/FetchError.js';
import { getResponseBody, isJson } from '../utils/response.js';
import BaseComponent from './BaseComponent.js';
import NillaInfo from './InfoDialog.js';

/**
 * Enum for identifiers to query DOM elements.
 * @readonly
 * @enum {string}
 */
const Elements = Object.freeze({
    Target: 'target',
    LoadingIndicator: 'loading-indicator',
    InfoDialog: 'info-dialog',
});

/**
 * Web component for pushState + ajax support.
 */
class PJax extends BaseComponent {
    /**
     * Version number to include with requests.
     * @type {string}
     */
    #version = '';

    /**
     * List of file types to ignore when intercepting links.
     * @type {string[]}
     */
    #ignoreFileTypes = ['pdf', 'doc', 'docx', 'zip', 'rar', '7z', 'gif', 'jpeg', 'jpg', 'png'];

    /**
     * Name of attribute that indicates an element should be ignored.
     * @type {string}
     */
    #excludeAttribute = 'data-pjax-no-follow';

    /**
     * Bound popState event listener.
     * @type {(event: any) => Promise<void>|undefined}
     */
    #popStateListener;

    /**
     * Bound click event listener.
     * @type {(event: any) => Promise<void>|undefined}
     */
    #clickListener;

    /**
     * Bound submit event listener.
     * @type {(event: any) => Promise<void>|undefined}
     */
    #submitListener;

    /**
     * Path of the current page to use when creating history.
     * @type {string}
     */
    #currentUrlForHistory;

    /**
     * Url origin.
     * @type {string}
     */
    #origin;

    /**
     * Initialize pjax by setting up event listeners.
     */
    constructor() {
        super();

        // set the prefix to use when querying/caching elements
        super.elementPrefix = 'pjax';

        if (this.dataset.version) {
            this.#version = this.dataset.version;
        }

        this.#popStateListener = event => this.#onPopState(event);
        this.#clickListener = event => this.#onClickListener(event);
        this.#submitListener = event => this.#onSubmitListener(event);

        window.addEventListener('popstate', this.#popStateListener);
        this.addEventListener('click', this.#clickListener);
        this.addEventListener('submit', this.#submitListener);

        // create a state object for the current page with out special properties
        window.history.replaceState({ url: document.location.href, title: document.title }, '');

        const currentUrl = new URL(document.location.href);
        this.#currentUrlForHistory = currentUrl.pathname;
        this.#origin = currentUrl.origin;
    }

    /**
     * Clean up when removing component.
     */
    disconnectedCallback() {
        if (this.#popStateListener) {
            window.removeEventListener('popstate', this.#popStateListener);
            this.#popStateListener = undefined;
        }
        if (this.#clickListener) {
            this.removeEventListener('click', this.#clickListener);
            this.#clickListener = undefined;
        }
        if (this.#submitListener) {
            this.removeEventListener('submit', this.#submitListener);
            this.#submitListener = undefined;
        }

        super.disconnectedCallback();
    }

    /**
     * Listens for back/forward navigation events and updates page accordingly.
     * @param {PopStateEvent} event PopState event
     */
    async #onPopState(event) {
        if (event.state !== null) {
            // stop if url is missing from state
            if (!(event.state.url && URL.canParse(event.state.url, this.#origin))) {
                return;
            }

            // If there is a state object, handle it as a page load.
            // history is only designed to work with GET requests.  don't want to try to store/rebuild request body for other methods like POST/PUT
            await this.#requestPage(new URL(event.state.url, this.#origin), HttpMethods.GET, false);
        }
    }

    /**
     * Link click listener.
     * @param {Event} event Click event
     */
    async #onClickListener(event) {
        let target = /** @type {HTMLAnchorElement} */ (event.target);
        if (target.nodeName !== 'A') {
            target = target.closest('a');
        }

        // Ignore clicks unless its a link
        if (!(target && target.nodeName === 'A')) {
            return;
        }

        // Ignore clicks if link has the exclude class
        if (this.#excludeAttribute && target.hasAttribute(this.#excludeAttribute)) {
            return;
        }

        if (!URL.canParse(target.href)) {
            return;
        }

        // Ignore external links
        const url = new URL(target.href);
        if (url.origin !== this.#origin) {
            return;
        }

        // Ignore anchors on the same page
        if (url.pathname === document.location.pathname && url.hash.length > 0) {
            return;
        }

        // Skip link if file type is within ignored types array
        if (this.#ignoreFileTypes.indexOf(url.pathname.split('.').pop().toLowerCase()) !== -1) {
            return;
        }

        // Don't fire normal event
        event.preventDefault();

        // Take no action if we are already on requested page
        if (document.location.href === target.href) {
            return;
        }

        let method = target.dataset.pjaxMethod;
        if (!(method && method.toUpperCase() in HttpMethods)) {
            method = HttpMethods.GET;
        }

        await this.#requestPage(url, method, true);
    }

    /**
     * Form submit listener.
     * @param {Event} event Submit event.
     */
    async #onSubmitListener(event) {
        let target = /** @type {HTMLFormElement} */ (event.target);
        if (target.nodeName !== 'FORM') {
            target = target.closest('form');
        }

        // Ignore submit unless its a form
        if (!(target && target.nodeName === 'FORM')) {
            return;
        }

        // Ignore submit if the form has the exclude attribute
        if (this.#excludeAttribute && target.hasAttribute(this.#excludeAttribute)) {
            return;
        }

        if (!URL.canParse(target.action)) {
            return;
        }

        // Ignore submit if the origin doesn't match
        const url = new URL(target.action);
        if (url.origin !== this.#origin) {
            return;
        }

        // Don't fire normal event
        event.preventDefault();

        let method = target.dataset.pjaxMethod;
        if (!(method && method.toUpperCase() in HttpMethods)) {
            method = target.method;
        }
        if (!(method && method.toUpperCase() in HttpMethods)) {
            method = HttpMethods.POST;
        }

        // handle the submission
        await this.#submitForm(url, method, new FormData(target));
    }

    /**
     * Performs request to load content from server and handles the result.
     * @param {URL} url Url to request.
     * @param {string} method Http method to use for request.
     * @param {boolean} updateHistory Update browser history if true.
     */
    async #requestPage(url, method, updateHistory) {
        this.#showLoadingIndicator();

        // keep browser from caching requests by tacking milliseconds to end of url
        url.searchParams.set('_t', `${Date.now()}`);

        try {
            const response = await fetch(url, {
                method,
                signal: AbortSignal.timeout(DefaultTimeout),
                headers: this.#buildRequestHeaders(),
            });

            if (!response.ok) {
                throw new FetchError(`HTTP ${response.status}: ${response.statusText}`);
            }

            await this.#processResponse(url, updateHistory, response);
        } catch (error) {
            await this.#handleResponseError(error);
        } finally {
            this.#hideLoadingIndicator();
        }
    }

    /**
     * Performs form submission to server and handles the result.
     * @param {URL} url Url to request.
     * @param {string} method Http method to use for request.
     * @param {FormData} formData Form element to sub
     */
    async #submitForm(url, method, formData) {
        this.#showLoadingIndicator();

        try {
            const response = await fetch(url, {
                method,
                signal: AbortSignal.timeout(DefaultTimeout),
                headers: this.#buildRequestHeaders(),
                body: formData,
            });

            if (!response.ok) {
                throw new FetchError(`HTTP ${response.status}: ${response.statusText}`);
            }

            await this.#processResponse(url, true, response);
        } catch (error) {
            await this.#handleResponseError(error);
        } finally {
            this.#hideLoadingIndicator();
        }
    }

    /**
     * Creates headers to use with requests to server.
     * @returns {object} Headers for fetch request
     */
    #buildRequestHeaders() {
        const headers = {};
        headers[HttpHeaders.RequestedWith] = 'XMLHttpRequest';
        headers[HttpHeaders.PJax] = true;
        headers[HttpHeaders.PJaxVersion] = this.#version;
        return headers;
    }

    /**
     * Process a response from the server.
     * @param {URL} requestUrl Requested url.
     * @param {boolean} updateHistory Update browser history if true.
     * @param {Response} response Fetch response.
     */
    async #processResponse(requestUrl, updateHistory, response) {
        const body = await getResponseBody(response);

        // reload the page if the refresh header was included
        if (response.headers.has(HttpHeaders.PJaxRefresh)) {
            document.location.href = requestUrl.href;
            return;
        }

        // make sure request is successful and response is HTML
        if (!response.ok || isJson(response)) {
            this.#showErrorDialog();
            return;
        }

        // make sure request is successful and response is HTML
        if (updateHistory) {
            const newUrl = response.headers.has(HttpHeaders.PJaxPushUrl) ? response.headers.get(HttpHeaders.PJaxPushUrl) : requestUrl.pathname;

            if (this.#currentUrlForHistory !== newUrl) {
                // don't add page to history unless the url changed
                this.#currentUrlForHistory = newUrl;

                // push the page into the history
                window.history.pushState({ url: this.#currentUrlForHistory, title: document.title }, '', this.#currentUrlForHistory);
            }
        }

        // update the DOM with the new content
        const targetElement = this.getElement(Elements.Target);
        if (targetElement) {
            // if the browser supports view transitions, use them to update the content
            if (document.startViewTransition) {
                document.startViewTransition(() => {
                    targetElement.innerHTML = body;
                });
            } else {
                targetElement.innerHTML = body;
            }
        }

        // set the document title if title header exists
        if (response.headers.has(HttpHeaders.PJaxTitle)) {
            document.title = response.headers.get(HttpHeaders.PJaxTitle);
        }

        window.scrollTo(0, 0);

        const elementToFocus = /** @type {HTMLElement} */ (targetElement.querySelector('[autofocus]'));
        elementToFocus?.focus();
    }

    /**
     * Shows the loading indicator.
     */
    #showLoadingIndicator() {
        this.getElement(Elements.LoadingIndicator)?.classList.add('pjax-request');
    }

    /**
     * Hides the loading indicator.
     */
    #hideLoadingIndicator() {
        this.getElement(Elements.LoadingIndicator)?.classList.remove('pjax-request');
    }

    /**
     * Handle response errors.
     * @param {FetchError} error Error from fetch.
     */
    async #handleResponseError(error) {
        console.error(error.message);
        this.#showErrorDialog();
    }

    /**
     * Shows the error dialog.
     */
    #showErrorDialog() {
        const element = /** @type {NillaInfo} */ (this.getElement(Elements.InfoDialog));
        element?.show();
    }
}

// Define the new web component
if ('customElements' in window) {
    customElements.define('nilla-pjax', PJax);
}

export default PJax;
