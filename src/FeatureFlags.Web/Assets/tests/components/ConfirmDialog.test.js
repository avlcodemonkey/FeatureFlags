/**
 * Unit tests for nilla-confirm.
 * Testing keyboard events is iffy, so focus/tab trap logic isn't tested.
 */

import assert from 'node:assert/strict';
import { beforeEach, describe, it } from 'node:test';
import isRendered from '../testUtils/isRendered.js';
import setupDom from '../testUtils/setupDom.js';
import tick from '../testUtils/tick.js';

// Setup jsdom first
await setupDom();

// Import the custom element after jsdom is set up
await import('../../js/components/ConfirmDialog.js');

const textContent = 'Dialog test';
const textOkay = 'okay';
const textCancel = 'cancel';
const valueOkay = 'ok';
const valueCancel = 'cancel';

const confirmDialogHtml = `
    <nilla-confirm>
        <button data-dialog-content="${textContent}" data-dialog-ok="${textOkay}" data-dialog-cancel="${textCancel}">open</button>
        <button data-missing-content data-dialog-ok="just an ok">do nothing</button>
        <button data-do-nothing>do nothing</button>
    </nilla-confirm>
`;

/**
 * Gets the confirm dialog custom element.
 * @returns {HTMLElement | null | undefined} Confirm dialog element
 */
function getConfirmDialog() {
    return document.body.querySelector('nilla-confirm');
}

/**
 * Gets the button to open the dialog.
 * @returns {HTMLElement | null | undefined} Open button
 */
function getOpenButton() {
    return getConfirmDialog()?.querySelector('[data-dialog-cancel]');
}

/**
 * Gets the button without the content attribute.
 * @returns {HTMLElement | null | undefined} Button
 */
function getBadButton() {
    return getConfirmDialog()?.querySelector('[data-missing-content]');
}

/**
 * Gets the button that should do nothing.
 * @returns {HTMLElement | null | undefined} Dismiss button
 */
function getDoNothingButton() {
    return getConfirmDialog()?.querySelector('[data-do-nothing]');
}

/**
 * Gets the dialog element after its created.
 * @returns {HTMLDialogElement | null} Dialog element
 */
function getNativeDialog() {
    return document.body.querySelector('dialog');
}

/**
 * Gets the okay button from the dialog.
 * @returns {HTMLButtonElement | null | undefined} Okay button
 */
function getOkayButton() {
    return getNativeDialog()?.querySelector(`button[value="${valueOkay}"]`);
}

/**
 * Gets the cancel button from the dialog.
 * @returns {HTMLButtonElement | null | undefined} Cancel button
 */
function getCancelButton() {
    return getNativeDialog()?.querySelector(`button[value="${valueCancel}"]`);
}

// Mocks for dialog methods
let dialogResponse;

/**
 * Sets up mock implementations for dialog methods and tracks calls.
 * Sets showModalCalled and closeCalled flags for test assertions.
 */
function setupDialogMocks() {
    HTMLDialogElement.prototype.show = () => {
    };
    HTMLDialogElement.prototype.showModal = () => {
        setupDialogMocks.showModalCalled = true;
    };
    HTMLDialogElement.prototype.close = function(returnValue) {
        setupDialogMocks.closeCalled = true;
        dialogResponse = returnValue;
    };
    setupDialogMocks.showModalCalled = false;
    setupDialogMocks.closeCalled = false;
}

/**
 * Restores dialog method mocks and resets call tracking flags.
 */
function restoreDialogMocks() {
    delete HTMLDialogElement.prototype.show;
    delete HTMLDialogElement.prototype.showModal;
    delete HTMLDialogElement.prototype.close;
    setupDialogMocks.showModalCalled = false;
    setupDialogMocks.closeCalled = false;
}

describe('nilla-confirm', () => {
    beforeEach(async () => {
        dialogResponse = undefined;
        document.body.innerHTML = confirmDialogHtml;
        setupDialogMocks();
        await isRendered(getConfirmDialog);
    });

    it('should open on open button click', async () => {
        const confirmDialog = getConfirmDialog();
        const openButton = getOpenButton();

        openButton?.click();
        await tick();
        const nativeDialog = getNativeDialog();

        assert.ok(confirmDialog, 'Confirm dialog should exist');
        assert.ok(openButton, 'Open button should exist');
        assert.ok(nativeDialog, 'Native dialog should exist');
        assert.ok(nativeDialog.innerHTML.includes(textContent), 'Dialog should contain content');
        assert.ok(setupDialogMocks.showModalCalled, 'showModal should be called');
        restoreDialogMocks();
    });

    it('should not open on bad button click', async () => {
        const confirmDialog = getConfirmDialog();
        const badButton = getBadButton();

        badButton?.click();
        await tick();
        const nativeDialog = getNativeDialog();

        assert.ok(confirmDialog, 'Confirm dialog should exist');
        assert.ok(badButton, 'Bad button should exist');
        assert.strictEqual(nativeDialog, null, 'Native dialog should not exist');
        assert.ok(!setupDialogMocks.showModalCalled, 'showModal should not be called');
        restoreDialogMocks();
    });

    it('should not open on do nothing button click', async () => {
        const confirmDialog = getConfirmDialog();
        const doNothingButton = getDoNothingButton();

        doNothingButton?.click();
        await tick();
        const nativeDialog = getNativeDialog();

        assert.ok(confirmDialog, 'Confirm dialog should exist');
        assert.ok(doNothingButton, 'Do nothing button should exist');
        assert.strictEqual(nativeDialog, null, 'Native dialog should not exist');
        assert.ok(!setupDialogMocks.showModalCalled, 'showModal should not be called');
        restoreDialogMocks();
    });

    it('should have content, okay button, and cancel button when open', async () => {
        const confirmDialog = getConfirmDialog();
        const openButton = getOpenButton();

        openButton?.click();
        await tick();
        const nativeDialog = getNativeDialog();
        const okayButton = getOkayButton();
        const cancelButton = getCancelButton();
        const p = nativeDialog.querySelector('p');

        assert.ok(confirmDialog, 'Confirm dialog should exist');
        assert.ok(openButton, 'Open button should exist');
        assert.ok(nativeDialog, 'Native dialog should exist');
        assert.ok(p, 'Dialog content should exist');
        assert.strictEqual(p.textContent, textContent, 'Dialog content should match');
        assert.ok(okayButton, 'Okay button should exist');
        assert.strictEqual(okayButton.textContent, textOkay, 'Okay button text should match');
        assert.ok(cancelButton, 'Cancel button should exist');
        assert.strictEqual(cancelButton.textContent, textCancel, 'Cancel button text should match');
        assert.ok(setupDialogMocks.showModalCalled, 'showModal should be called');
        assert.ok(!setupDialogMocks.closeCalled, 'close should not be called');
        restoreDialogMocks();
    });

    it('should close dialog when clicking okay', async () => {
        const confirmDialog = getConfirmDialog();
        const openButton = getOpenButton();

        openButton?.click();
        await tick();
        const nativeDialog = getNativeDialog();
        const okayButton = getOkayButton();
        okayButton?.click();
        await tick();

        assert.ok(confirmDialog, 'Confirm dialog should exist');
        assert.ok(openButton, 'Open button should exist');
        assert.ok(nativeDialog, 'Native dialog should exist');
        assert.ok(okayButton, 'Okay button should exist');
        assert.strictEqual(dialogResponse, valueOkay, 'Dialog response should be okay');
        assert.ok(setupDialogMocks.showModalCalled, 'showModal should be called');
        assert.ok(setupDialogMocks.closeCalled, 'close should be called');
        restoreDialogMocks();
    });

    it('should close dialog when clicking cancel', async () => {
        const confirmDialog = getConfirmDialog();
        const openButton = getOpenButton();

        openButton?.click();
        await tick();
        const nativeDialog = getNativeDialog();
        const cancelButton = getCancelButton();
        cancelButton?.click();
        await tick();

        assert.ok(confirmDialog, 'Confirm dialog should exist');
        assert.ok(openButton, 'Open button should exist');
        assert.ok(nativeDialog, 'Native dialog should exist');
        assert.ok(cancelButton, 'Cancel button should exist');
        assert.strictEqual(dialogResponse, valueCancel, 'Dialog response should be cancel');
        assert.ok(setupDialogMocks.showModalCalled, 'showModal should be called');
        assert.ok(setupDialogMocks.closeCalled, 'close should be called');
        restoreDialogMocks();
    });

    it('should not open a second dialog if already open', async () => {
        const openButton = getOpenButton();
        openButton?.click();
        await tick();

        // Try to open again while dialog is open
        openButton?.click();
        await tick();

        const dialogs = document.body.querySelectorAll('dialog');
        assert.strictEqual(dialogs.length, 1, 'Only one dialog should exist');
        restoreDialogMocks();
    });

    it('should not close dialog when clicking outside dialog', async () => {
        const openButton = getOpenButton();
        openButton?.click();
        await tick();
        const nativeDialog = getNativeDialog();

        // Simulate click outside dialog
        const event = new MouseEvent('click', { bubbles: true });
        document.body.dispatchEvent(event);
        await tick();

        assert.ok(nativeDialog, 'Dialog should still exist after outside click');
        assert.ok(!setupDialogMocks.closeCalled, 'Dialog should not close on outside click');
        restoreDialogMocks();
    });
});
