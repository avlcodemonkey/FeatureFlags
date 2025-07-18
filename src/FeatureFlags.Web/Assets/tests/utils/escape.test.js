/**
 * Unit tests for escape function.
 */

import {
    describe, expect, it,
} from 'vitest';
import escape from '../../js/utils/escape';

describe('escape', () => {
    it('escapes &, <, >, ", \', /, `, =', () => {
        const input = `&<>"'/\`=`;
        const expected = '&amp;&lt;&gt;&quot;&#39;&#x2F;&#x60;&#x3D;';
        expect(escape(input)).toBe(expected);
    });

    it('returns the same string if no escapable characters', () => {
        expect(escape('hello world')).toBe('hello world');
    });

    it('escapes only the special characters', () => {
        expect(escape('a&b<c>d"e\'f/g`h=i')).toBe('a&amp;b&lt;c&gt;d&quot;e&#39;f&#x2F;g&#x60;h&#x3D;i');
    });

    it('handles empty string', () => {
        expect(escape('')).toBe('');
    });

    it('converts non-string input to string and escapes', () => {
        expect(escape(1234)).toBe('1234');
        expect(escape(null)).toBe('null');
        expect(escape(undefined)).toBe('undefined');
        expect(escape(true)).toBe('true');
    });
});
