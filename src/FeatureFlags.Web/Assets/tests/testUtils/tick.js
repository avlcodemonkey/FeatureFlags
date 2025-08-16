/**
 * Creates a promise that resolves after a tick.
 * @returns {Promise} Promise to wait for.
 */
async function tick() {
    return new Promise((resolve) => {
        setTimeout(resolve);
    });
}

export default tick;
