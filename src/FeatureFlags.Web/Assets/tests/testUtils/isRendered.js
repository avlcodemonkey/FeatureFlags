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

export default isRendered;
