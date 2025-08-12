/**
 * Unit tests for prettyPrintJson function.
 */

import { describe, it, expect } from 'vitest';
import { prettyPrintJson } from '../../js/utils/prettyPrintJson';

describe('prettyPrintJson', () => {
    it('should pretty print a simple object', () => {
        const obj = { foo: 'bar', num: 42 };
        const html = prettyPrintJson.toHtml(obj);
        expect(html).toContain('class="json-key"');
        expect(html).toContain('class="json-string"');
        expect(html).toContain('class="json-number"');
        expect(html).toContain('foo');
        expect(html).toContain('bar');
        expect(html).toContain('42');
    });

    it('should escape HTML special characters', () => {
        const obj = { x: '<script>&</script>' };
        const html = prettyPrintJson.toHtml(obj);
        expect(html).toContain('&lt;script&gt;&amp;&lt;/script&gt;');
    });

    it('should handle null and booleans', () => {
        const obj = { a: null, b: true, c: false };
        const html = prettyPrintJson.toHtml(obj);
        expect(html).toContain('class="json-null"');
        expect(html).toContain('class="json-boolean"');
        expect(html).toContain('null');
        expect(html).toContain('true');
        expect(html).toContain('false');
    });

    it('should pretty print arrays', () => {
        const arr = [1, 'two', false];
        const html = prettyPrintJson.toHtml(arr);
        expect(html).toContain('class="json-number"');
        expect(html).toContain('class="json-string"');
        expect(html).toContain('class="json-boolean"');
        expect(html).toContain('two');
        expect(html).toContain('false');
    });

    it('should use custom indent', () => {
        const obj = { foo: 'bar' };
        const html = prettyPrintJson.toHtml(obj, { indent: 2 });
        // Should contain two spaces for indentation
        expect(html).toContain('  <span');
    });

    it('should quote keys if quoteKeys is true', () => {
        const obj = { foo: 'bar' };
        const html = prettyPrintJson.toHtml(obj, { quoteKeys: true });
        // Should contain quotes around the key
        expect(html).toMatch(/"foo"/);
    });

    it('should add trailing commas if trailingCommas is true', () => {
        const obj = { foo: 'bar', baz: 1 };
        const html = prettyPrintJson.toHtml(obj, { trailingCommas: true });
        // Should contain a comma after the last property
        expect(html).toMatch(
            /<span class="json-key">"baz"<\/span><span class="json-mark">: <\/span><span class="json-number">1<\/span><span class="json-mark">,<\/span>/,
        );
    });

    it('should return "undefined" for undefined input', () => {
        const html = prettyPrintJson.toHtml(undefined);
        expect(html).toContain('undefined');
    });
});
