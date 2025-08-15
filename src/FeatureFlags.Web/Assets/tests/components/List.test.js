/**
 * Unit tests for nilla-list.
 */

import { describe, it, beforeEach } from 'node:test';
import assert from 'node:assert/strict';
import setupDom from '../testUtils/setupDom.js';
import tick from '../testUtils/tick.js';
import isRendered from '../testUtils/isRendered.js';

// Setup jsdom first
await setupDom();

// Import the custom element AFTER jsdom is set up
await import('../../js/components/List.js');

const itemHtml = `
    <div data-list-item>
        <span>Item</span>
        <button data-list-remove-button>Remove</button>
    </div>
`;

const listHtml = `
    <nilla-list>
        <template data-list-template>
            ${itemHtml}
        </template>
        <div data-list-container></div>
        <span data-list-empty-message class="is-hidden">No items</span>
        <button data-list-add-button>Add</button>
    </nilla-list>
`;

/**
 * Gets the list element.
 * @returns {HTMLElement | null | undefined} List element
 */
function getList() {
    return document.body.querySelector('nilla-list');
}

/**
 * Gets the add button.
 * @returns {HTMLElement | null | undefined} Add button
 */
function getAddButton() {
    return getList()?.querySelector('[data-list-add-button]');
}

/**
 * Gets the container for items.
 * @returns {HTMLElement | null | undefined} Container element
 */
function getContainer() {
    return getList()?.querySelector('[data-list-container]');
}

/**
 * Gets the empty message element.
 * @returns {HTMLElement | null | undefined} Empty message element
 */
function getEmptyMessage() {
    return getList()?.querySelector('[data-list-empty-message]');
}

/**
 * Gets all item elements.
 * @returns {HTMLElement[]} Item elements
 */
function getItems() {
    return getContainer()?.querySelectorAll('[data-list-item]') ?? [];
}

describe('nilla-list', () => {
    beforeEach(async () => {
        document.body.innerHTML = listHtml;
        await isRendered(getList);
    });

    it('shows empty message when no items', async () => {
        const emptyMessage = getEmptyMessage();
        assert.ok(emptyMessage, 'Empty message element should exist');
        assert.ok(!emptyMessage.classList.contains('is-hidden'), 'Empty message should be visible');
    });

    it('adds item on add button click', async () => {
        const addButton = getAddButton();
        addButton?.click();
        await tick();

        const items = getItems();
        assert.strictEqual(items.length, 1, 'Should have one item after add');

        const emptyMessage = getEmptyMessage();
        assert.ok(emptyMessage.classList.contains('is-hidden'), 'Empty message should be hidden after add');
    });

    it('removes item on remove button click', async () => {
        const addButton = getAddButton();
        addButton?.click();
        await tick();

        let items = getItems();
        assert.strictEqual(items.length, 1, 'Should have one item after add');

        const removeButton = items[0].querySelector('[data-list-remove-button]');
        removeButton?.click();
        await tick();

        items = getItems();
        assert.strictEqual(items.length, 0, 'Should have zero items after remove');

        const emptyMessage = getEmptyMessage();
        assert.ok(!emptyMessage.classList.contains('is-hidden'), 'Empty message should be visible after remove');
    });

    it('hides empty message when item is present', async () => {
        const addButton = getAddButton();
        addButton?.click();
        await tick();

        const emptyMessage = getEmptyMessage();
        assert.ok(emptyMessage.classList.contains('is-hidden'), 'Empty message should be hidden when item is present');
    });
});
