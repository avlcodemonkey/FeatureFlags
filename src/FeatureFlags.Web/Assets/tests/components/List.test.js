/**
 * Unit tests for nilla-list.
 */

import {
    beforeEach, describe, expect, it,
} from 'vitest';
import { tick, isRendered } from '../utils';
import '../../js/components/List';

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
 * @returns {NodeListOf<HTMLElement>} Item elements
 */
function getItems() {
    return getContainer()?.querySelectorAll('[data-list-item]') ?? [];
}

describe('nilla-list', async () => {
    beforeEach(async () => {
        document.body.innerHTML = listHtml;
        await isRendered(getList);
    });

    it('should show empty message when no items', async () => {
        const emptyMessage = getEmptyMessage();
        expect(emptyMessage).toBeTruthy();
        expect(emptyMessage.classList).not.toContain('is-hidden');
    });

    it('should add item on add button click', async () => {
        const addButton = getAddButton();
        addButton?.click();
        await tick();

        const items = getItems();
        expect(items.length).toBe(1);

        const emptyMessage = getEmptyMessage();
        expect(emptyMessage.classList).toContain('is-hidden');
    });

    it('should remove item on remove button click', async () => {
        const addButton = getAddButton();
        addButton?.click();
        await tick();

        let items = getItems();
        expect(items.length).toBe(1);

        const removeButton = items[0].querySelector('[data-list-remove-button]');
        removeButton?.click();
        await tick();

        items = getItems();
        expect(items.length).toBe(0);

        const emptyMessage = getEmptyMessage();
        expect(emptyMessage.classList).not.toContain('is-hidden');
    });

    it('should hide empty message when item is present', async () => {
        const addButton = getAddButton();
        addButton?.click();
        await tick();

        const emptyMessage = getEmptyMessage();
        expect(emptyMessage.classList).toContain('is-hidden');
    });
});
