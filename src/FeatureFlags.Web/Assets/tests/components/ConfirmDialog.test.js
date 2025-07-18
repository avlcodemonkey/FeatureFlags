/**
 * Unit tests for nilla-confirm.
 * Testing keyboard events is iffy, so focus/tab trap logic isn't tested.
 */

import {
    beforeEach, beforeAll, describe, expect, it, vi, afterEach,
} from 'vitest';
import { tick, isRendered } from '../utils';
import '../../js/components/ConfirmDialog';

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
 * @returns {HTMLElement | null | undefined} Button
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

describe('confirm dialog', async () => {
    let dialogResponse;

    beforeEach(async () => {
        dialogResponse = undefined;
        document.body.innerHTML = confirmDialogHtml;
        await isRendered(getConfirmDialog);
    });

    beforeAll(() => {
        // workaround since jsdom doesn't support modals yet
        HTMLDialogElement.prototype.show = vi.fn();
        HTMLDialogElement.prototype.showModal = vi.fn();
        HTMLDialogElement.prototype.close = vi.fn((returnValue) => {
            dialogResponse = returnValue;
        });
    });

    afterEach(() => {
        vi.restoreAllMocks();
    });

    it('should open on open button click', async () => {
        const confirmDialog = getConfirmDialog();
        const openButton = getOpenButton();
        const spy = vi.spyOn(HTMLDialogElement.prototype, 'showModal');

        openButton?.click();
        await tick();
        const nativeDialog = getNativeDialog();

        expect(confirmDialog).toBeTruthy();
        expect(openButton).toBeTruthy();
        expect(nativeDialog).toBeTruthy();
        expect(nativeDialog.innerHTML).toContain(textContent);
        expect(spy).toHaveBeenCalledTimes(1);
    });

    it('should not open on bad button click', async () => {
        const confirmDialog = getConfirmDialog();
        const badButton = getBadButton();
        const spy = vi.spyOn(HTMLDialogElement.prototype, 'showModal');

        badButton?.click();
        await tick();
        const nativeDialog = getNativeDialog();

        expect(confirmDialog).toBeTruthy();
        expect(badButton).toBeTruthy();
        expect(nativeDialog).toBeFalsy();
        expect(spy).toHaveBeenCalledTimes(0);
    });

    it('should not open on do nothing button click', async () => {
        const confirmDialog = getConfirmDialog();
        const doNothingButton = getDoNothingButton();
        const spy = vi.spyOn(HTMLDialogElement.prototype, 'showModal');

        doNothingButton?.click();
        await tick();
        const nativeDialog = getNativeDialog();

        expect(confirmDialog).toBeTruthy();
        expect(doNothingButton).toBeTruthy();
        expect(nativeDialog).toBeFalsy();
        expect(spy).toHaveBeenCalledTimes(0);
    });

    it('should have content, okay button, and cancel button when open', async () => {
        const confirmDialog = getConfirmDialog();
        const openButton = getOpenButton();
        const spyShow = vi.spyOn(HTMLDialogElement.prototype, 'showModal');
        const spyClose = vi.spyOn(HTMLDialogElement.prototype, 'close');

        openButton?.click();
        await tick();
        const nativeDialog = getNativeDialog();
        const okayButton = getOkayButton();
        const cancelButton = getCancelButton();
        const p = nativeDialog.querySelector('p');

        expect(confirmDialog).toBeTruthy();
        expect(openButton).toBeTruthy();
        expect(nativeDialog).toBeTruthy();
        expect(p).toBeTruthy();
        expect(p.textContent).toEqual(textContent);
        expect(okayButton).toBeTruthy();
        expect(okayButton.textContent).toEqual(textOkay);
        expect(cancelButton).toBeTruthy();
        expect(cancelButton.textContent).toEqual(textCancel);
        expect(spyShow).toHaveBeenCalledTimes(1);
        expect(spyClose).toHaveBeenCalledTimes(0);
    });

    it('should close dialog when clicking okay', async () => {
        const confirmDialog = getConfirmDialog();
        const openButton = getOpenButton();
        const spyShow = vi.spyOn(HTMLDialogElement.prototype, 'showModal');
        const spyClose = vi.spyOn(HTMLDialogElement.prototype, 'close');

        openButton?.click();
        await tick();
        const nativeDialog = getNativeDialog();
        const okayButton = getOkayButton();
        okayButton?.click();
        await tick();

        expect(confirmDialog).toBeTruthy();
        expect(openButton).toBeTruthy();
        expect(nativeDialog).toBeTruthy();
        expect(okayButton).toBeTruthy();
        expect(dialogResponse).toEqual(valueOkay);
        expect(spyShow).toHaveBeenCalledTimes(1);
        // jsdom doesn't support modals yet, so check our mock was called instead of checking that getNativeDialog() is falsy
        expect(spyClose).toHaveBeenCalledTimes(1);
    });

    it('should close dialog when clicking cancel', async () => {
        const confirmDialog = getConfirmDialog();
        const openButton = getOpenButton();
        const spyShow = vi.spyOn(HTMLDialogElement.prototype, 'showModal');
        const spyClose = vi.spyOn(HTMLDialogElement.prototype, 'close');

        openButton?.click();
        await tick();
        const nativeDialog = getNativeDialog();
        const cancelButton = getCancelButton();
        cancelButton?.click();
        await tick();

        expect(confirmDialog).toBeTruthy();
        expect(openButton).toBeTruthy();
        expect(nativeDialog).toBeTruthy();
        expect(cancelButton).toBeTruthy();
        expect(dialogResponse).toEqual(valueCancel);
        expect(spyShow).toHaveBeenCalledTimes(1);
        // jsdom doesn't support modals yet, so check our mock was called instead of checking that getNativeDialog() is falsy
        expect(spyClose).toHaveBeenCalledTimes(1);
    });
});
