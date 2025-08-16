import { JSDOM } from 'jsdom';

/**
 * Sets up a JSDOM environment with minimal HTML and initializes the global window and document objects.
 * @param {string} html - Optional HTML string to be parsed and set as the document body.
 * @returns {Promise<void>} A promise that resolves when the DOM is fully loaded and global objects are set.
 */
async function setupDom(html) {
    html ??= `<!DOCTYPE html><html><head></head><body></body></html>`;
    const dom = new JSDOM(html, {
        runScripts: 'dangerously',
        resources: 'usable',
    });

    return new Promise((resolve) => {
        dom.window.addEventListener('load', () => {
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

            resolve(dom);
        });
    });
}

export default setupDom;
