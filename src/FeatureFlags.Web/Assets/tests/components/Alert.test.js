/**
 * Unit tests for nilla-alert.
 */

import assert from 'node:assert/strict';
import { beforeEach, describe, it } from 'node:test';
import isRendered from '../testUtils/isRendered.js';
import setupDom from '../testUtils/setupDom.js';
import tick from '../testUtils/tick.js';

// Setup jsdom first
await setupDom();

// Import the custom element after jsdom is set up
await import('../../js/components/Alert.js');

const textContent = 'Alert test';

const dismissableAlertHtml = `
    <nilla-alert>
        <div>
            <span>${textContent}</span>
            <span><button data-dismiss>close</button></span>
            <span><button data-do-nothing>do nothing</button></span>
        </div>
    </nilla-alert>
`;

const notDismissableAlertHtml = `
    <nilla-alert>
        <div>
            <span>${textContent}</span>
            <span><button data-do-nothing>do nothing</button></span>
        </div>
    </nilla-alert>
`;

/**
 * Gets the alert element.
 * @returns {HTMLElement | null | undefined} Alert element
 */
function getAlert() {
    return document.body.querySelector('nilla-alert');
}

/**
 * Gets the button to dismiss the alert.
 * @returns {HTMLElement | null | undefined} Dismiss button
 */
function getDismissButton() {
    return getAlert()?.querySelector('[data-dismiss]');
}

/**
 * Gets the button that should do nothing.
 * @returns {HTMLElement | null | undefined} Dismiss button
 */
function getDoNothingButton() {
    return getAlert()?.querySelector('[data-do-nothing]');
}

describe('dismissable alert', () => {
    beforeEach(async () => {
        document.body.innerHTML = dismissableAlertHtml;
        await isRendered(getAlert);
    });

    it('should have test text', async () => {
        const alert = getAlert();
        assert.ok(alert.innerHTML.includes(textContent), 'Alert should contain test text');
    });

    it('should hide on dismiss button click', async () => {
        const alert = getAlert();
        const dismissButton = getDismissButton();

        dismissButton?.click();
        await tick();

        assert.ok(alert, 'Alert should exist');
        assert.ok(dismissButton, 'Dismiss button should exist');
        assert.ok(alert.classList.contains('is-hidden'), 'Alert should be hidden after dismiss');
    });

    it('should not hide on do nothing button click', async () => {
        const alert = getAlert();
        const doNothingButton = getDoNothingButton();

        doNothingButton?.click();
        await tick();

        assert.ok(alert, 'Alert should exist');
        assert.ok(doNothingButton, 'Do nothing button should exist');
        assert.ok(!alert.classList.contains('is-hidden'), 'Alert should not be hidden after do nothing');
    });
});

describe('not dismissable alert', () => {
    beforeEach(async () => {
        document.body.innerHTML = notDismissableAlertHtml;
        await isRendered(getAlert);
    });

    it('should have test text', async () => {
        const alert = getAlert();
        assert.ok(alert.innerHTML.includes(textContent), 'Alert should contain test text');
    });

    it('should have no dismiss button', async () => {
        const dismissButton = getDismissButton();
        assert.ok(!dismissButton, 'Dismiss button should not exist');
    });

    it('should not hide on do nothing button click', async () => {
        const alert = getAlert();
        const doNothingButton = getDoNothingButton();

        doNothingButton?.click();
        await tick();

        assert.ok(alert, 'Alert should exist');
        assert.ok(doNothingButton, 'Do nothing button should exist');
        assert.ok(!alert.classList.contains('is-hidden'), 'Alert should not be hidden after do nothing');
    });
});
