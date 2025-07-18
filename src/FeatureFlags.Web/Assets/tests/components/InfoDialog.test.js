/**
 * Unit tests for nilla-info.
 * Testing keyboard events is iffy, so focus/tab trap logic isn't tested.
 */

import {
    beforeEach, beforeAll, describe, expect, it, vi, afterEach,
} from 'vitest';
import { tick, isRendered } from '../utils';
import '../../js/components/InfoDialog';

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

describe('info dialog', async () => {
    beforeEach(async () => {
        document.body.innerHTML = infoDialogHtml;
        await isRendered(getInfoDialog);
    });

    beforeAll(() => {
        // workaround since jsdom doesn't support modals yet
        HTMLDialogElement.prototype.show = vi.fn();
        HTMLDialogElement.prototype.showModal = vi.fn();
        HTMLDialogElement.prototype.close = vi.fn();
    });

    afterEach(() => {
        vi.restoreAllMocks();
    });

    it('should open on open button click', async () => {
        const infoDialog = getInfoDialog();
        const openButton = getOpenButton();
        const spy = vi.spyOn(HTMLDialogElement.prototype, 'showModal');

        openButton?.click();
        await tick();
        const nativeDialog = getNativeDialog();

        expect(infoDialog).toBeTruthy();
        expect(openButton).toBeTruthy();
        expect(nativeDialog).toBeTruthy();
        expect(nativeDialog.innerHTML).toContain(textContent);
        expect(spy).toHaveBeenCalledTimes(1);
    });

    it('should not open on bad button click', async () => {
        const infoDialog = getInfoDialog();
        const badButton = getBadButton();
        const spy = vi.spyOn(HTMLDialogElement.prototype, 'showModal');

        badButton?.click();
        await tick();
        const nativeDialog = getNativeDialog();

        expect(infoDialog).toBeTruthy();
        expect(badButton).toBeTruthy();
        expect(nativeDialog).toBeFalsy();
        expect(spy).toHaveBeenCalledTimes(0);
    });

    it('should not open on do nothing button click', async () => {
        const infoDialog = getInfoDialog();
        const doNothingButton = getDoNothingButton();
        const spy = vi.spyOn(HTMLDialogElement.prototype, 'showModal');

        doNothingButton?.click();
        await tick();
        const nativeDialog = getNativeDialog();

        expect(infoDialog).toBeTruthy();
        expect(doNothingButton).toBeTruthy();
        expect(nativeDialog).toBeFalsy();
        expect(spy).toHaveBeenCalledTimes(0);
    });

    it('should have content and okay button when open', async () => {
        const infoDialog = getInfoDialog();
        const openButton = getOpenButton();
        const spyShow = vi.spyOn(HTMLDialogElement.prototype, 'showModal');
        const spyClose = vi.spyOn(HTMLDialogElement.prototype, 'close');

        openButton?.click();
        await tick();
        const nativeDialog = getNativeDialog();
        const okayButton = getOkayButton();
        const cancelButton = getCancelButton();
        const p = nativeDialog.querySelector('p');

        expect(infoDialog).toBeTruthy();
        expect(openButton).toBeTruthy();
        expect(nativeDialog).toBeTruthy();
        expect(p).toBeTruthy();
        expect(p.textContent).toEqual(textContent);
        expect(okayButton).toBeTruthy();
        expect(okayButton.textContent).toEqual(textOkay);
        expect(cancelButton).toBeFalsy();
        expect(spyShow).toHaveBeenCalledTimes(1);
        expect(spyClose).toHaveBeenCalledTimes(0);
    });

    it('should close dialog when clicking okay', async () => {
        const infoDialog = getInfoDialog();
        const openButton = getOpenButton();
        const spyShow = vi.spyOn(HTMLDialogElement.prototype, 'showModal');
        const spyClose = vi.spyOn(HTMLDialogElement.prototype, 'close');

        openButton?.click();
        await tick();
        const nativeDialog = getNativeDialog();
        const okayButton = getOkayButton();
        okayButton?.click();
        await tick();

        expect(infoDialog).toBeTruthy();
        expect(openButton).toBeTruthy();
        expect(nativeDialog).toBeTruthy();
        expect(okayButton).toBeTruthy();
        expect(spyShow).toHaveBeenCalledTimes(1);
        // jsdom doesn't support modals yet, so check our mock was called instead of checking that getNativeDialog() is falsy
        expect(spyClose).toHaveBeenCalledTimes(1);
    });
});
