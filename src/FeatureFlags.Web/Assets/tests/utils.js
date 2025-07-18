/**
 * Creates a promise that resolves after a tick.
 * @returns {Promise} Promise to wait for.
 */
async function tick() {
    return new Promise((resolve) => {
        setTimeout(resolve);
    });
}

/**
 * Creates a promise that resolves when the getElementFn returns true.
 * @param {*} getElementFn Function to get the element that we are waiting to be rendered.
 * @returns {Promise} Promise to wait for.
 */
function isRendered(getElementFn) {
    return new Promise((resolve) => {
        const interval = setInterval(() => {
            if (getElementFn()) {
                clearInterval(interval);
                resolve();
            }
        });
    });
}

export { tick, isRendered };
