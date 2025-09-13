/**
 * Unit tests for nilla-list.
 */

import assert from 'node:assert/strict';
import { beforeEach, describe, it } from 'node:test';
import isRendered from '../testUtils/isRendered.js';
import setupDom from '../testUtils/setupDom.js';
import tick from '../testUtils/tick.js';

// Setup jsdom first
await setupDom();

// Import the custom element after jsdom is set up
await import('../../js/components/List.js');

const itemHtmlWithPlaceholder = `
    <div data-list-item data-index="{{index}}">
        <span>Item</span>
        <button data-list-remove-button>Remove</button>
    </div>
`;

const listHtmlWithPlaceholder = `
    <nilla-list data-list-index-placeholder="{{index}}">
        <template data-list-template>
            ${itemHtmlWithPlaceholder}
        </template>
        <div data-list-container></div>
        <span data-list-empty-message class="is-hidden">No items</span>
        <button data-list-add-button>Add</button>
    </nilla-list>
`;

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
    describe('without indexPlaceholder', () => {
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

        it('adds multiple items sequentially', async () => {
            const addButton = getAddButton();
            addButton?.click();
            await tick();
            addButton?.click();
            await tick();
            addButton?.click();
            await tick();

            const items = getItems();
            assert.strictEqual(items.length, 3, 'Should have three items after three adds');

            const emptyMessage = getEmptyMessage();
            assert.ok(emptyMessage.classList.contains('is-hidden'), 'Empty message should be hidden when items are present');
        });

        it('removes items one by one until empty', async () => {
            const addButton = getAddButton();
            for (let i = 0; i < 3; i++) {
                addButton?.click();
                await tick();
            }

            let items = getItems();
            assert.strictEqual(items.length, 3, 'Should have three items after adds');

            // Remove items one by one
            for (let i = 0; i < 3; i++) {
                items = getItems();
                const removeButton = items[0].querySelector('[data-list-remove-button]');
                removeButton?.click();
                await tick();
            }

            items = getItems();
            assert.strictEqual(items.length, 0, 'Should have zero items after removing all');

            const emptyMessage = getEmptyMessage();
            assert.ok(!emptyMessage.classList.contains('is-hidden'), 'Empty message should be visible after all items removed');
        });

        it('removes only the clicked item when multiple exist', async () => {
            const addButton = getAddButton();
            for (let i = 0; i < 3; i++) {
                addButton?.click();
                await tick();
            }

            let items = getItems();
            assert.strictEqual(items.length, 3, 'Should have three items after adds');

            // Remove the second item
            const secondRemoveButton = items[1].querySelector('[data-list-remove-button]');
            secondRemoveButton?.click();
            await tick();

            items = getItems();
            assert.strictEqual(items.length, 2, 'Should have two items after removing one');
        });

        it('does not add item if add button is disabled', async () => {
            const addButton = getAddButton();
            addButton.disabled = true;
            addButton.click();
            await tick();

            const items = getItems();
            assert.strictEqual(items.length, 0, 'Should not add item when add button is disabled');
        });

        it('does not throw if remove button is clicked when no items', async () => {
            const container = getContainer();
            // Simulate clicking a remove button when no items exist
            const fakeRemoveButton = document.createElement('button');
            fakeRemoveButton.setAttribute('data-list-remove-button', '');
            container.appendChild(fakeRemoveButton);

            assert.doesNotThrow(() => {
                fakeRemoveButton.click();
            }, 'Clicking remove button with no items should not throw');

            container.removeChild(fakeRemoveButton);
        });
    });

    describe('with indexPlaceholder', () => {
        beforeEach(async () => {
            document.body.innerHTML = listHtmlWithPlaceholder;
            await isRendered(getList);
        });

        it('replaces indexPlaceholder with unique values for each item', async () => {
            const addButton = getAddButton();
            addButton?.click();
            await tick();
            addButton?.click();
            await tick();
            addButton?.click();
            await tick();

            const items = getItems();
            assert.strictEqual(items.length, 3, 'Should have three items after three adds');

            // Collect all index values
            const indices = Array.from(items).map(item => item.getAttribute('data-index'));
            // All should be non-empty and unique
            assert.ok(indices.every(idx => idx && idx.length > 0), 'All items should have a non-empty index');
            const uniqueIndices = new Set(indices);
            assert.strictEqual(uniqueIndices.size, 3, 'All indices should be unique');
        });

        it('does not replace indexPlaceholder if not present in template', async () => {
            // Remove indexPlaceholder from template and re-render
            document.body.innerHTML = listHtml;
            await isRendered(getList);
            const addButton = getAddButton();
            addButton?.click();
            await tick();

            const items = getItems();
            assert.strictEqual(items.length, 1, 'Should have one item after add');
            assert.strictEqual(items[0].getAttribute('data-index'), null, 'Item should not have data-index attribute');
        });

        it('removes items and maintains unique indices for remaining items', async () => {
            const addButton = getAddButton();
            addButton?.click();
            await tick();
            addButton?.click();
            await tick();
            addButton?.click();
            await tick();

            let items = getItems();
            const indicesBefore = Array.from(items).map(item => item.getAttribute('data-index'));
            assert.strictEqual(new Set(indicesBefore).size, 3, 'All indices should be unique before remove');

            // Remove the second item
            const secondRemoveButton = items[1].querySelector('[data-list-remove-button]');
            secondRemoveButton?.click();
            await tick();

            items = getItems();
            const indicesAfter = Array.from(items).map(item => item.getAttribute('data-index'));
            assert.strictEqual(items.length, 2, 'Should have two items after removing one');
            assert.strictEqual(new Set(indicesAfter).size, 2, 'Remaining indices should be unique');
            // The removed index should not be present
            assert.ok(!indicesAfter.includes(indicesBefore[1]), 'Removed index should not be present');
        });
    });
});
