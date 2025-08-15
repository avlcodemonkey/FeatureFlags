/**
 * Unit tests for response util functions.
 */

import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import { getContentType, isJson, getResponseBody } from '../../js/utils/response.js';

describe('getContentType', () => {
    it('should return value when content type is set', async () => {
        const headerValue = 'test';
        const response = new Response();
        response.headers.set('content-type', headerValue);

        const result = getContentType(response);

        assert.strictEqual(result, headerValue);
    });

    it('should return empty string when no content type is set', async () => {
        const response = new Response();

        const result = getContentType(response);

        assert.strictEqual(result, '');
    });

    it('should handle case-insensitive header name', async () => {
        const headerValue = 'application/json';
        const response = new Response();
        response.headers.set('Content-Type', headerValue); // capitalized

        const result = getContentType(response);

        assert.strictEqual(result, headerValue);
    });

    it('should return empty string if response is null', async () => {
        const result = getContentType(null);
        assert.strictEqual(result, '');
    });
});

describe('isJson', () => {
    it('should return true when content type is json', async () => {
        const headerValue = 'application/json';
        const response = new Response();
        response.headers.set('content-type', headerValue);

        const result = isJson(response);

        assert.ok(result);
    });

    it('should return true for case-insensitive content type', async () => {
        const headerValue = 'Application/Json';
        const response = new Response();
        response.headers.set('content-type', headerValue);

        const result = isJson(response);

        assert.ok(result);
    });

    it('should return false when no content type is set', async () => {
        const response = new Response();

        const result = isJson(response);

        assert.ok(!result);
    });

    it('should return false when content type is not json', async () => {
        const headerValue = 'application/text';
        const response = new Response();
        response.headers.set('content-type', headerValue);

        const result = isJson(response);

        assert.ok(!result);
    });

    it('should return false if response is null', async () => {
        const result = isJson(null);
        assert.ok(!result);
    });
});

describe('getResponseBody', () => {
    it('should return json when content type is json', async () => {
        const headerValue = 'application/json';
        const obj = { result: true };
        const response = new Response(JSON.stringify(obj));
        response.headers.set('content-type', headerValue);

        const result = await getResponseBody(response);

        assert.ok(result);
        assert.deepStrictEqual(result, { result: true });
    });

    it('should return json for case-insensitive content type', async () => {
        const headerValue = 'Application/Json';
        const obj = { result: true };
        const response = new Response(JSON.stringify(obj));
        response.headers.set('content-type', headerValue);

        const result = await getResponseBody(response);

        assert.ok(result);
        assert.deepStrictEqual(result, { result: true });
    });

    it('should throw error for malformed JSON', async () => {
        const headerValue = 'application/json';
        const response = new Response('{"invalidJson": }');
        response.headers.set('content-type', headerValue);

        let errorThrown = false;
        try {
            await getResponseBody(response);
        } catch (e) {
            errorThrown = true;
            assert.ok(e instanceof Error);
        }
        assert.ok(errorThrown);
    });

    it('should return html when content type is html', async () => {
        const headerValue = 'application/html';
        const html = '<html></html>';
        const response = new Response(html);
        response.headers.set('content-type', headerValue);

        const result = await getResponseBody(response);

        assert.ok(result);
        assert.strictEqual(result, html);
    });

    it('should return html when no content type is set', async () => {
        const html = '<html></html>';
        const response = new Response(html);

        const result = await getResponseBody(response);

        assert.ok(result);
        assert.strictEqual(result, html);
    });

    it('should return empty string for empty response body', async () => {
        const response = new Response('');
        response.headers.set('content-type', 'application/html');

        const result = await getResponseBody(response);

        assert.strictEqual(result, '');
    });

    it('should throw error if response is null', async () => {
        let errorThrown = false;
        try {
            await getResponseBody(null);
        } catch (e) {
            errorThrown = true;
            assert.ok(e instanceof Error);
        }
        assert.ok(errorThrown);
    });
});
