/**
 * Unit tests for response util functions.
 */

import {
    describe, expect, it,
} from 'vitest';
import { getContentType, isJson, getResponseBody } from '../../js/utils/response';

describe('getContentType', async () => {
    it('should return value when content type is set', async () => {
        const headerValue = 'test';
        const response = new Response();
        response.headers.set('content-type', headerValue);

        const result = getContentType(response);

        expect(result).toEqual(headerValue);
    });

    it('should return empty string when no content type is set', async () => {
        const response = new Response();

        const result = getContentType(response);

        expect(result).toEqual('');
    });
});

describe('isJson', async () => {
    it('should return true when content type is json', async () => {
        const headerValue = 'application/json';
        const response = new Response();
        response.headers.set('content-type', headerValue);

        const result = isJson(response);

        expect(result).toBeTruthy();
    });

    it('should return false when no content type is set', async () => {
        const response = new Response();

        const result = isJson(response);

        expect(result).toBeFalsy();
    });

    it('should return false when content type is not json', async () => {
        const headerValue = 'application/text';
        const response = new Response();
        response.headers.set('content-type', headerValue);

        const result = isJson(response);

        expect(result).toBeFalsy();
    });
});

describe('getResponseBody', async () => {
    it('should return json when content type is json', async () => {
        const headerValue = 'application/json';
        const obj = { result: true };
        const response = new Response(JSON.stringify(obj));
        response.headers.set('content-type', headerValue);

        const result = await getResponseBody(response);

        expect(result).toBeTruthy();
        expect(result).toMatchObject({ result: true });
    });

    it('should return html when content type is html', async () => {
        const headerValue = 'application/html';
        const html = '<html></html>';
        const response = new Response(html);
        response.headers.set('content-type', headerValue);

        const result = await getResponseBody(response);

        expect(result).toBeTruthy();
        expect(result).toEqual(html);
    });

    it('should return html when no content type is set', async () => {
        const html = '<html></html>';
        const response = new Response(html);

        const result = await getResponseBody(response);

        expect(result).toBeTruthy();
        expect(result).toEqual(html);
    });
});
