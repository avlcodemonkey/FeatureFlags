/**
 * Unit tests for indexedCompare function.
 */

import { describe, it } from 'node:test';
import assert from 'node:assert';
import indexedCompare from '../../js/utils/indexedCompare.js';

describe('indexedCompare', () => {
    it('returns 0 when _index values are equal', () => {
        const a = { _index: 5 };
        const b = { _index: 5 };
        assert.strictEqual(indexedCompare(a, b), 0);
    });

    it('returns -1 when first _index is less than second', () => {
        const a = { _index: 3 };
        const b = { _index: 7 };
        assert.strictEqual(indexedCompare(a, b), -1);
    });

    it('returns 1 when first _index is greater than second', () => {
        const a = { _index: 10 };
        const b = { _index: 2 };
        assert.strictEqual(indexedCompare(a, b), 1);
    });

    it('does nothing when missing _index property', () => {
        const a = {};
        const b = { _index: 0 };
        assert.strictEqual(indexedCompare(a, b), 0);

        const c = { _index: 0 };
        const d = {};
        assert.strictEqual(indexedCompare(c, d), 0);

        const e = {};
        const f = {};
        assert.strictEqual(indexedCompare(e, f), 0);
    });

    it('works with negative _index values', () => {
        const a = { _index: -5 };
        const b = { _index: 0 };
        assert.strictEqual(indexedCompare(a, b), -1);

        const c = { _index: -2 };
        const d = { _index: -10 };
        assert.strictEqual(indexedCompare(c, d), 1);
    });
});
