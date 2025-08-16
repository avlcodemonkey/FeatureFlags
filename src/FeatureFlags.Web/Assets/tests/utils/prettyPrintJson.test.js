/**
 * Unit tests for prettyPrintJson function.
 */

import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import { prettyPrintJson } from '../../js/utils/prettyPrintJson.js';

describe('prettyPrintJson', () => {
    it('should pretty print a simple object', () => {
        const obj = { foo: 'bar', num: 42 };
        const html = prettyPrintJson.toHtml(obj);
        assert.ok(html.includes('class="json-key"'));
        assert.ok(html.includes('class="json-string"'));
        assert.ok(html.includes('class="json-number"'));
        assert.ok(html.includes('foo'));
        assert.ok(html.includes('bar'));
        assert.ok(html.includes('42'));
    });

    it('should escape HTML special characters', () => {
        const obj = { x: '<script>&</script>' };
        const html = prettyPrintJson.toHtml(obj);
        assert.ok(html.includes('&lt;script&gt;&amp;&lt;/script&gt;'));
    });

    it('should handle null and booleans', () => {
        const obj = { a: null, b: true, c: false };
        const html = prettyPrintJson.toHtml(obj);
        assert.ok(html.includes('class="json-null"'));
        assert.ok(html.includes('class="json-boolean"'));
        assert.ok(html.includes('null'));
        assert.ok(html.includes('true'));
        assert.ok(html.includes('false'));
    });

    it('should pretty print arrays', () => {
        const arr = [1, 'two', false];
        const html = prettyPrintJson.toHtml(arr);
        assert.ok(html.includes('class="json-number"'));
        assert.ok(html.includes('class="json-string"'));
        assert.ok(html.includes('class="json-boolean"'));
        assert.ok(html.includes('two'));
        assert.ok(html.includes('false'));
    });

    it('should use custom indent', () => {
        const obj = { foo: 'bar' };
        const html = prettyPrintJson.toHtml(obj, { indent: 2 });
        assert.ok(html.includes('  <span'));
    });

    it('should quote keys if quoteKeys is true', () => {
        const obj = { foo: 'bar' };
        const html = prettyPrintJson.toHtml(obj, { quoteKeys: true });
        assert.match(html, /"foo"/);
    });

    it('should add trailing commas if trailingCommas is true', () => {
        const obj = { foo: 'bar', baz: 1 };
        const html = prettyPrintJson.toHtml(obj, { trailingCommas: true });
        assert.match(
            html,
            /<span class="json-key">"baz"<\/span><span class="json-mark">: <\/span><span class="json-number">1<\/span><span class="json-mark">,<\/span>/,
        );
    });

    it('should return "undefined" for undefined input', () => {
        const html = prettyPrintJson.toHtml(undefined);
        assert.ok(html.includes('undefined'));
    });

    it('should handle circular references gracefully', () => {
        const obj = {};
        obj.self = obj;
        let html;
        try {
            html = prettyPrintJson.toHtml(obj);
        } catch (e) {
            html = String(e);
        }
        // Should not throw, should mention circular or error
        assert.ok(html.includes('circular') || html.includes('error') || html.includes('[Circular]'));
    });

    it('should handle function values', () => {
        const obj = {
            fn: function() {
                // do nothing
            },
        };
        const html = prettyPrintJson.toHtml(obj);
        assert.ok(html.includes('<span class="json-mark">{}</span>'));
    });

    it('should handle empty object', () => {
        const obj = {};
        const html = prettyPrintJson.toHtml(obj);
        assert.ok(html.includes('{}'));
    });

    it('should handle empty array', () => {
        const arr = [];
        const html = prettyPrintJson.toHtml(arr);
        assert.ok(html.includes('[]'));
    });

    it('should handle large object', () => {
        const obj = {};
        for (let i = 0; i < 1000; i++) {
            obj['key' + i] = i;
        }
        const html = prettyPrintJson.toHtml(obj);
        assert.ok(html.includes('key999'));
        assert.ok(html.includes('999'));
    });

    it('should handle deeply nested object', () => {
        let obj = { a: 1 };
        for (let i = 0; i < 20; i++) {
            obj = { nest: obj };
        }
        const html = prettyPrintJson.toHtml(obj);
        assert.ok(html.includes('nest'));
        assert.ok(html.includes('1'));
    });

    it('should handle Date objects', () => {
        const obj = { date: new Date('2020-01-01T00:00:00Z') };
        const html = prettyPrintJson.toHtml(obj);
        assert.ok(html.includes('2020'));
    });

    it('should handle RegExp objects', () => {
        const obj = { regex: /abc/i };
        const html = prettyPrintJson.toHtml(obj);
        assert.ok(html.includes('<span class="json-key">"regex"</span><span class="json-mark">: </span><span class="json-mark">{}</span>'));
    });

    it('should handle NaN and Infinity', () => {
        const obj = { nan: NaN, inf: Infinity, ninf: -Infinity };
        const html = prettyPrintJson.toHtml(obj);
        assert.ok(!html.includes('NaN'));
        assert.ok(!html.includes('Infinity'));
        assert.ok(!html.includes('-Infinity'));
    });
});
