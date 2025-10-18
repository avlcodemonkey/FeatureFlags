import { JSDOM } from 'jsdom';

/**
 * Sets up a JSDOM environment with minimal HTML and initializes the global window and document objects.
 * @param {string} html - Optional HTML string to be parsed and set as the document body.
 * @param {string} [baseUrl] - Optional base URL for the JSDOM environment.
 * @returns {Promise<void>} A promise that resolves when the DOM is fully loaded and global objects are set.
 */
async function setupDom(html, baseUrl) {
    html ??= `<!DOCTYPE html><html><head></head><body></body></html>`;
    const dom = new JSDOM(html, {
        runScripts: 'dangerously',
        resources: 'usable',
        url: baseUrl ?? 'http://localhost', // Set a default URL for relative paths
    });

    return new Promise((resolve) => {
        dom.window.addEventListener('load', () => {
            // Set global objects to the JSDOM window and document
            global.window ??= dom.window;
            global.document ??= dom.window.document;
            global.HTMLElement ??= dom.window.HTMLElement;
            global.HTMLInputElement ??= dom.window.HTMLInputElement;
            global.HTMLDialogElement ??= dom.window.HTMLDialogElement;
            global.FormData = dom.window.FormData;
            global.customElements ??= dom.window.customElements;
            global.Event ??= dom.window.Event;
            global.KeyboardEvent ??= dom.window.KeyboardEvent;
            global.MouseEvent ??= dom.window.MouseEvent;
            global.PopStateEvent ??= dom.window.PopStateEvent;
            global.CustomEvent ??= dom.window.CustomEvent;
            global.sessionStorage ??= dom.window.sessionStorage;

            // Mock window.scrollTo to prevent errors in tests since jsdom does not implement it
            window.scrollTo = () => {
                // do nothing
            };

            resolve(dom);
        });
    });
}

export default setupDom;
