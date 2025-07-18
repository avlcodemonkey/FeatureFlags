/**
 * Enum for custom http request/response headers.
 * @readonly
 * @enum {string}
 */
const HttpHeaders = Object.freeze({
    PJax: 'x-pjax',
    PJaxTitle: 'x-pjax-title',
    PJaxPushUrl: 'x-pjax-push-url',
    PJaxRefresh: 'x-pjax-refresh',
    PJaxVersion: 'x-pjax-version',
    RequestedWith: 'x-requested-with',
});

export default HttpHeaders;
