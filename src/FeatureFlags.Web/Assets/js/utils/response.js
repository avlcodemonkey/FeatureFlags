/**
 * Get content type from reponse.
 * @param {Response} response Response object to get type from.
 * @returns {string} Content type.
 */
function getContentType(response) {
    return response?.headers.get('content-type')?.toLowerCase() ?? '';
}

/**
 * Check if response is json.
 * @param {Response} response Response object to check.
 * @returns {boolean} True if response is json.
 */
function isJson(response) {
    return getContentType(response).indexOf('application/json') > -1;
}

/**
 * Get the body from the response.
 * @param {Response} response  Response object to get body from.
 * @returns {Promise<string>} Response content.
 */
async function getResponseBody(response) {
    const body = await (getContentType(response).indexOf('application/json') > -1 ? response.json() : response.text());
    return body;
}

export { getContentType, isJson, getResponseBody };
