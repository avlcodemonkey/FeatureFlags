import { JSDOM } from 'jsdom';

/**
 * Sets up a JSDOM environment with minimal HTML and initializes the global window and document objects.
 * @returns {Promise<void>} A promise that resolves when the DOM is fully loaded and global objects are set.
 */
async function setupDom() {
    const dom = new JSDOM(`<!DOCTYPE html><html><head></head><body></body></html>`, {
        runScripts: 'dangerously',
        resources: 'usable',
    });

    return new Promise((resolve) => {
        dom.window.addEventListener('load', () => {
            global.window ??= dom.window;
            global.document ??= dom.window.document;
            global.HTMLElement ??= dom.window.HTMLElement;
            global.HTMLInputElement ??= dom.window.HTMLInputElement;
            global.FormData = dom.window.FormData;

            resolve(dom);
        });
    });
}

export default setupDom;
