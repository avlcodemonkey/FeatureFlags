/**
 * Unit tests for filterArray function.
 */

import { describe, it } from 'node:test';
import assert from 'node:assert';
import filterArray from '../../js/utils/filterArray.js';

describe('filterArray', () => {
    it('filters objects containing the search string in any property', () => {
        const arr = [
            { name: 'Alice', city: 'Wonderland' },
            { name: 'Bob', city: 'London' },
            { name: 'Charlie', city: 'Paris' },
        ];
        const result = arr.filter(filterArray.bind(null, 'Alice'));
        assert.deepStrictEqual(result, [{ name: 'Alice', city: 'Wonderland' }]);
    });

    it('is case insensitive', () => {
        const arr = [
            { name: 'Alice', city: 'Wonderland' },
            { name: 'Bob', city: 'London' },
        ];
        const result = arr.filter(filterArray.bind(null, 'alice'));
        assert.deepStrictEqual(result, [{ name: 'Alice', city: 'Wonderland' }]);
    });

    it('filters with multiple tokens', () => {
        const arr = [
            { name: 'Alice Wonderland', city: 'London' },
            { name: 'Wonderland Alice', city: 'Paris' },
            { name: 'Bob', city: 'Wonderland' },
        ];
        const result = arr.filter(filterArray.bind(null, 'Alice Wonderland'));
        assert.deepStrictEqual(result, [
            { name: 'Alice Wonderland', city: 'London' },
            { name: 'Wonderland Alice', city: 'Paris' },
        ]);
    });

    it('returns all objects for empty search string', () => {
        const arr = [
            { name: 'Alice', city: 'Wonderland' },
            { name: 'Bob', city: 'London' },
        ];
        const result = arr.filter(filterArray.bind(null, ''));
        assert.deepStrictEqual(result, arr);
    });

    it('returns empty array when no objects match', () => {
        const arr = [
            { name: 'Alice', city: 'Wonderland' },
            { name: 'Bob', city: 'London' },
        ];
        const result = arr.filter(filterArray.bind(null, 'Charlie'));
        assert.deepStrictEqual(result, []);
    });

    it('filters with partial matches', () => {
        const arr = [
            { name: 'Alice', city: 'Wonderland' },
            { name: 'Bob', city: 'London' },
        ];
        const result = arr.filter(filterArray.bind(null, 'Ali'));
        assert.deepStrictEqual(result, [{ name: 'Alice', city: 'Wonderland' }]);
    });

    it('returns empty array for empty input array', () => {
        const arr = [];
        const result = arr.filter(filterArray.bind(null, 'Alice'));
        assert.deepStrictEqual(result, []);
    });
});
