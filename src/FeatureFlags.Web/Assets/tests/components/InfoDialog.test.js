/**
 * Unit tests for nilla-info.
 * Testing keyboard events is iffy, so focus/tab trap logic isn't tested.
 */

import assert from 'node:assert/strict';
import { beforeEach, describe, it, afterEach } from 'node:test';
import isRendered from '../testUtils/isRendered.js';
import setupDom from '../testUtils/setupDom.js';
import tick from '../testUtils/tick.js';

// Setup jsdom first
await setupDom();

// Import the custom element after jsdom is set up
await import('../../js/components/InfoDialog.js');

const textContent = 'Dialog test';
const textOkay = 'okay';
const valueOkay = 'ok';
const valueCancel = 'cancel';

const infoDialogHtml = `
    <nilla-info>
        <button data-dialog-content="${textContent}" data-dialog-ok="${textOkay}">open</button>
        <button data-missing-content data-dialog-ok="just an ok">do nothing</button>
        <button data-do-nothing>do nothing</button>
    </nilla-info>
`;

/**
 * Gets the info dialog custom element.
 * @returns {HTMLElement | null | undefined} Info dialog element
 */
function getInfoDialog() {
    return document.body.querySelector('nilla-info');
}

/**
 * Gets the button to open the dialog.
 * @returns {HTMLElement | null | undefined} Open button
 */
function getOpenButton() {
    return getInfoDialog()?.querySelector('[data-dialog-content]');
}

/**
 * Gets the button without the content attribute.
 * @returns {HTMLElement | null | undefined} Button
 */
function getBadButton() {
    return getInfoDialog()?.querySelector('[data-missing-content]');
}

/**
 * Gets the button that should do nothing.
 * @returns {HTMLElement | null | undefined} Dismiss button
 */
function getDoNothingButton() {
    return getInfoDialog()?.querySelector('[data-do-nothing]');
}

/**
 * Gets the dialog element after its created.
 * @returns {HTMLElement | null | undefined} Dialog
 */
function getNativeDialog() {
    return document.body.querySelector('dialog');
}

/**
 * Gets the okay button from the dialog.
 * @returns {HTMLElement | null | undefined} Button
 */
function getOkayButton() {
    return getNativeDialog()?.querySelector(`button[value="${valueOkay}"]`);
}

/**
 * Gets the cancel button from the dialog.
 * @returns {HTMLElement | null | undefined} Button
 */
function getCancelButton() {
    return getNativeDialog()?.querySelector(`button[value="${valueCancel}"]`);
}

// Save original dialog methods
const originalShow = HTMLDialogElement.prototype.show;
const originalShowModal = HTMLDialogElement.prototype.showModal;
const originalClose = HTMLDialogElement.prototype.close;

describe('info dialog', () => {
    beforeEach(async () => {
        // Mock dialog methods since jsdom doesn't support modals
        HTMLDialogElement.prototype.show = () => {
        };
        HTMLDialogElement.prototype.showModal = () => {
            HTMLDialogElement.prototype._showModalCalled = (HTMLDialogElement.prototype._showModalCalled || 0) + 1;
        };
        HTMLDialogElement.prototype.close = () => {
            HTMLDialogElement.prototype._closeCalled = (HTMLDialogElement.prototype._closeCalled || 0) + 1;
        };
        document.body.innerHTML = infoDialogHtml;
        await isRendered(getInfoDialog);

        // Reset call counters
        HTMLDialogElement.prototype._showModalCalled = 0;
        HTMLDialogElement.prototype._closeCalled = 0;
    });

    afterEach(async () => {
        // Mock dialog methods since jsdom doesn't support modals
        HTMLDialogElement.prototype.show = originalShow;
        HTMLDialogElement.prototype.showModal = originalShowModal;
        HTMLDialogElement.prototype.close = originalClose;
    });

    it('should open on open button click', async () => {
        const infoDialog = getInfoDialog();
        const openButton = getOpenButton();

        openButton?.click();
        await tick();
        const nativeDialog = getNativeDialog();

        assert.ok(infoDialog, 'Info dialog element should exist');
        assert.ok(openButton, 'Open button should exist');
        assert.ok(nativeDialog, 'Native dialog should exist');
        assert.ok(nativeDialog.innerHTML.includes(textContent), 'Dialog should contain expected content');
        assert.strictEqual(HTMLDialogElement.prototype._showModalCalled, 1, 'showModal should be called once');
    });

    it('should not open on bad button click', async () => {
        const infoDialog = getInfoDialog();
        const badButton = getBadButton();

        badButton?.click();
        await tick();
        const nativeDialog = getNativeDialog();

        assert.ok(infoDialog, 'Info dialog element should exist');
        assert.ok(badButton, 'Bad button should exist');
        assert.ok(!nativeDialog, 'Native dialog should not exist');
        assert.strictEqual(HTMLDialogElement.prototype._showModalCalled, 0, 'showModal should not be called');
    });

    it('should not open on do nothing button click', async () => {
        const infoDialog = getInfoDialog();
        const doNothingButton = getDoNothingButton();

        doNothingButton?.click();
        await tick();
        const nativeDialog = getNativeDialog();

        assert.ok(infoDialog, 'Info dialog element should exist');
        assert.ok(doNothingButton, 'Do nothing button should exist');
        assert.ok(!nativeDialog, 'Native dialog should not exist');
        assert.strictEqual(HTMLDialogElement.prototype._showModalCalled, 0, 'showModal should not be called');
    });

    it('should have content and okay button when open', async () => {
        const infoDialog = getInfoDialog();
        const openButton = getOpenButton();

        openButton?.click();
        await tick();
        const nativeDialog = getNativeDialog();
        const okayButton = getOkayButton();
        const cancelButton = getCancelButton();
        const p = nativeDialog?.querySelector('p');

        assert.ok(infoDialog, 'Info dialog element should exist');
        assert.ok(openButton, 'Open button should exist');
        assert.ok(nativeDialog, 'Native dialog should exist');
        assert.ok(p, 'Dialog should contain a <p> element');
        assert.strictEqual(p.textContent, textContent, 'Dialog <p> should contain expected text');
        assert.ok(okayButton, 'Okay button should exist');
        assert.strictEqual(okayButton.textContent, textOkay, 'Okay button should have expected text');
        assert.ok(!cancelButton, 'Cancel button should not exist');
        assert.strictEqual(HTMLDialogElement.prototype._showModalCalled, 1, 'showModal should be called once');
        assert.strictEqual(HTMLDialogElement.prototype._closeCalled, 0, 'close should not be called yet');
    });

    it('should close dialog when clicking okay', async () => {
        const infoDialog = getInfoDialog();
        const openButton = getOpenButton();

        openButton?.click();
        await tick();
        const nativeDialog = getNativeDialog();
        const okayButton = getOkayButton();
        okayButton?.click();
        await tick();

        assert.ok(infoDialog, 'Info dialog element should exist');
        assert.ok(openButton, 'Open button should exist');
        assert.ok(nativeDialog, 'Native dialog should exist');
        assert.ok(okayButton, 'Okay button should exist');
        assert.strictEqual(HTMLDialogElement.prototype._showModalCalled, 1, 'showModal should be called once');
        assert.strictEqual(HTMLDialogElement.prototype._closeCalled, 1, 'close should be called once');
    });
});
