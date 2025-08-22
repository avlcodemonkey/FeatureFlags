/**
 * Unit tests for sortedCompare function.
 */

import { describe, it } from 'node:test';
import assert from 'node:assert';
import sortedCompare from '../../js/utils/sortedCompare.js';
import SortDirection from '../../js/constants/SortDirection.js';

describe('sortedCompare', () => {
    it('returns 0 when sorts is empty', () => {
        const a = { name: 'A' };
        const b = { name: 'B' };
        assert.strictEqual(sortedCompare([], a, b), 0);
    });

    it('sorts ascending by property', () => {
        const sorts = [{ property: 'name', direction: SortDirection.Asc }];
        const a = { name: 'A' };
        const b = { name: 'B' };
        assert.strictEqual(sortedCompare(sorts, a, b), -1);
        assert.strictEqual(sortedCompare(sorts, b, a), 1);
        assert.strictEqual(sortedCompare(sorts, a, a), 0);
    });

    it('sorts descending by property', () => {
        const sorts = [{ property: 'name', direction: SortDirection.Desc }];
        const a = { name: 'A' };
        const b = { name: 'B' };
        assert.strictEqual(sortedCompare(sorts, a, b), 1);
        assert.strictEqual(sortedCompare(sorts, b, a), -1);
        assert.strictEqual(sortedCompare(sorts, a, a), 0);
    });

    it('handles null values', () => {
        const sorts = [{ property: 'name', direction: SortDirection.Asc }];
        const a = { name: null };
        const b = { name: 'B' };
        assert.strictEqual(sortedCompare(sorts, a, b), 1);
        assert.strictEqual(sortedCompare(sorts, b, a), -1);
    });

    it('sorts by multiple properties', () => {
        const sorts = [
            { property: 'name', direction: SortDirection.Asc },
            { property: 'age', direction: SortDirection.Desc },
        ];
        const a = { name: 'A', age: 30 };
        const b = { name: 'A', age: 20 };
        assert.strictEqual(sortedCompare(sorts, a, b), -1); // age descending
        assert.strictEqual(sortedCompare(sorts, b, a), 1);
    });
});
