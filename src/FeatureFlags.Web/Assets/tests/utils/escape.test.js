/**
 * Unit tests for escape function.
 */

import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import escape from '../../js/utils/escape.js';

describe('escape', () => {
    it('escapes &, <, >, ", \', /, `, =', () => {
        const input = `&<>"'/\`=`;
        const expected = '&amp;&lt;&gt;&quot;&#39;&#x2F;&#x60;&#x3D;';
        assert.strictEqual(escape(input), expected);
    });

    it('returns the same string if no escapable characters', () => {
        assert.strictEqual(escape('hello world'), 'hello world');
    });

    it('escapes only the special characters', () => {
        assert.strictEqual(
            escape('a&b<c>d"e\'f/g`h=i'),
            'a&amp;b&lt;c&gt;d&quot;e&#39;f&#x2F;g&#x60;h&#x3D;i',
        );
    });

    it('handles empty string', () => {
        assert.strictEqual(escape(''), '');
    });

    it('converts non-string input to string and escapes', () => {
        assert.strictEqual(escape(1234), '1234');
        assert.strictEqual(escape(null), 'null');
        assert.strictEqual(escape(undefined), 'undefined');
        assert.strictEqual(escape(true), 'true');
    });
});
