import assert from 'node:assert/strict';
import { describe, it, beforeEach } from 'node:test';
import { findClosestComponent } from '../../js/utils/findClosestComponent.js';
import setupDom from '../testUtils/setupDom.js';

// Setup jsdom first
await setupDom();

describe('findClosestComponent', () => {
    let root, child, grandchild, unrelated;

    beforeEach(() => {
        document.body.innerHTML = '';
        // Create a custom component tree
        root = document.createElement('nilla-root');
        child = document.createElement('div');
        grandchild = document.createElement('span');
        unrelated = document.createElement('div');

        root.appendChild(child);
        child.appendChild(grandchild);
        document.body.appendChild(root);
        document.body.appendChild(unrelated);
    });

    it('returns the element itself if nodeName matches', () => {
        assert.strictEqual(findClosestComponent(root, 'NILLA-ROOT'), root);
    });

    it('finds the closest ancestor with the given nodeName', () => {
        assert.strictEqual(findClosestComponent(grandchild, 'NILLA-ROOT'), root);
        assert.strictEqual(findClosestComponent(child, 'NILLA-ROOT'), root);
    });

    it('returns null if no matching ancestor exists', () => {
        assert.strictEqual(findClosestComponent(unrelated, 'NILLA-ROOT'), null);
        assert.strictEqual(findClosestComponent(document.body, 'NILLA-ROOT'), null);
    });

    it('returns null if input node is null or undefined', () => {
        assert.strictEqual(findClosestComponent(null, 'NILLA-ROOT'), null);
        assert.strictEqual(findClosestComponent(undefined, 'NILLA-ROOT'), null);
    });

    it('handles deeply nested structures', () => {
        const deep = document.createElement('div');
        grandchild.appendChild(deep);
        assert.strictEqual(findClosestComponent(deep, 'NILLA-ROOT'), root);
    });
});
