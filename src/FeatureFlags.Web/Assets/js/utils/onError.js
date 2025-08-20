/**
 * Log errors to the backend.
 * @param {string} msg Message associated with the error
 * @param {string} url URL of the script or document associated with the error
 * @param {number} lineNum Line number if available
 * @param {number} columnNum Column number if available
 * @param {Error} error Error object for this error
 */
async function onError(msg, url, lineNum, columnNum, error) {
    if (!msg) {
        return;
    }

    const body = new FormData();
    body.append('message', msg);
    body.append('url', document.location.href);
    body.append('stack', error?.stack);

    // save error message to server
    try {
        navigator.sendBeacon('/Error/LogJavascriptError', body);
    } catch (ex) {
        console.error(`Unable to log error to server: ${msg}`);
        console.error(ex);
    }
}

window.onerror = onError;
