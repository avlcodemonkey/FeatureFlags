/**
 * Unit tests for nilla-display-toggle.
 */

import assert from 'node:assert/strict';
import { beforeEach, describe, it } from 'node:test';
import isRendered from '../testUtils/isRendered.js';
import setupDom from '../testUtils/setupDom.js';
import tick from '../testUtils/tick.js';

// Setup jsdom first
await setupDom();

// Import the custom element after jsdom is set up
await import('../../js/components/DisplayToggle.js');

const html = `
    <nilla-display-toggle>
        <select data-display-toggle-trigger>
            <option value="one" data-display-toggle-target="one">One</option>
            <option value="two" data-display-toggle-target="two">Two</option>
            <option value="three">Three</option>
        </select>
        <div data-display-toggle="one">Content One</div>
        <div data-display-toggle="two">Content Two</div>
        <div data-display-toggle="one,two">Content Both</div>
    </nilla-display-toggle>
`;

/**
 * Gets the nilla-display-toggle custom element from the DOM.
 * @returns {HTMLElement | null | undefined} The display toggle component element.
 */
function getComponent() {
    return document.body.querySelector('nilla-display-toggle');
}

/**
 * Gets the select element that acts as the display toggle trigger.
 * @returns {HTMLSelectElement | null | undefined} The trigger select element.
 */
function getTrigger() {
    return getComponent()?.querySelector('[data-display-toggle-trigger]');
}

/**
 * Gets the content element for "one".
 * @returns {HTMLElement | null | undefined} The content element for "one".
 */
function getContentOne() {
    return getComponent()?.querySelector('div[data-display-toggle="one"]');
}

/**
 * Gets the content element for "two".
 * @returns {HTMLElement | null | undefined} The content element for "two".
 */
function getContentTwo() {
    return getComponent()?.querySelector('div[data-display-toggle="two"]');
}

/**
 * Gets the content element for "one,two".
 * @returns {HTMLElement | null | undefined} The content element for "one,two".
 */
function getContentBoth() {
    return getComponent()?.querySelector('div[data-display-toggle="one,two"]');
}

describe('DisplayToggle', () => {
    /**
     * Sets up the DOM with the display toggle HTML before each test.
     * @returns {Promise<void>}
     */
    beforeEach(async () => {
        document.body.innerHTML = html;
        await isRendered(getComponent);
    });

    it('should show only content for selected option on load', async () => {
        const trigger = getTrigger();
        trigger.selectedIndex = 0; // "one"
        trigger.dispatchEvent(new window.Event('change'));
        await tick();

        assert.ok(!getContentOne().classList.contains('is-hidden'), 'Content One should be visible');
        assert.ok(getContentTwo().classList.contains('is-hidden'), 'Content Two should be hidden');
        assert.ok(!getContentBoth().classList.contains('is-hidden'), 'Content Both should be visible');
    });

    it('should show only content for second option when selected', async () => {
        const trigger = getTrigger();
        trigger.selectedIndex = 1; // "two"
        trigger.dispatchEvent(new window.Event('change'));
        await tick();

        assert.ok(getContentOne().classList.contains('is-hidden'), 'Content One should be hidden');
        assert.ok(!getContentTwo().classList.contains('is-hidden'), 'Content Two should be visible');
        assert.ok(!getContentBoth().classList.contains('is-hidden'), 'Content Both should be visible');
    });

    it('should hide all content if no option matches', async () => {
        const trigger = getTrigger();
        trigger.selectedIndex = 2;
        trigger.dispatchEvent(new window.Event('change'));
        await tick();

        assert.ok(getContentOne().classList.contains('is-hidden'), 'Content One should be hidden');
        assert.ok(getContentTwo().classList.contains('is-hidden'), 'Content Two should be hidden');
        assert.ok(getContentBoth().classList.contains('is-hidden'), 'Content Both should be hidden');
    });

    it('should handle multiple targets in data-display-toggle', async () => {
        const trigger = getTrigger();
        trigger.selectedIndex = 0; // "one"
        trigger.dispatchEvent(new window.Event('change'));
        await tick();
        assert.ok(!getContentBoth().classList.contains('is-hidden'), 'Content Both should be visible for "one"');

        trigger.selectedIndex = 1; // "two"
        trigger.dispatchEvent(new window.Event('change'));
        await tick();
        assert.ok(!getContentBoth().classList.contains('is-hidden'), 'Content Both should be visible for "two"');
    });

    it('should not affect elements outside the component', async () => {
        const outside = document.createElement('div');
        outside.setAttribute('data-display-toggle', 'one');
        outside.textContent = 'Outside Content';
        document.body.appendChild(outside);

        const trigger = getTrigger();
        trigger.selectedIndex = 0; // "one"
        trigger.dispatchEvent(new window.Event('change'));
        await tick();

        assert.ok(!outside.classList.contains('is-hidden'), 'Outside content should not be hidden by component');
    });
});
