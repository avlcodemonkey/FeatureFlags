/**
 * Unit tests for Luxbar toggle functionality.
 */

import {
    describe, expect, it, beforeEach,
} from 'vitest';
import { isRendered, tick } from './utils';
import setupLuxbarToggle from '../js/luxbar';

// luxbar-checkbox being checked by default simulates the menu being open on a mobile device
const menuHtml = `
    <div class="luxbar" data-luxbar>
        <input type="checkbox" id="luxbar-checkbox" checked data-checkbox>
        <div class="luxbar-menu">
            <ul class="luxbar-navigation">
                <li class="luxbar-header">
                    <a class="luxbar-brand" href="#" data-brand>Brand</a>
                    <label class="luxbar-hamburger" for="luxbar-checkbox"> <span></span> </label>
                </li>
                <li class="luxbar-active" data-active-link><a href="http://localhost/">Home</a></li>
                <li class="luxbar-dropdown" data-dropdown><a href="#">Users</a>
                    <ul>
                        <li><a href="#">Max</a></li>
                        <li><a href="#">Edgar</a></li>
                        <li data-child-link><a href="http://localhost/">John</a></li>
                    </ul>
                </li>
            </ul>
        </div>
    </div>
`;

/**
 * Gets the root element of the luxbar menu.
 * @returns {HTMLDivElement|null} Menu element.
 */
function getMenu() {
    return document.querySelector('[data-luxbar]');
}

/**
 * Gets the checkbox element that controls the menu state.
 * @returns {HTMLInputElement|null} Checkbox input.
 */
function getCheckbox() {
    return getMenu().querySelector('[data-checkbox]');
}

/**
 * Gets the active menu item link.
 * @returns {HTMLElement|null} Menu item link.
 */
function getActiveLink() {
    return getMenu().querySelector('[data-active-link] a');
}

/**
 * Gets the dropdown menu item link.
 * @returns {HTMLElement|null} Menu item link.
 */
function getDropdownLink() {
    return getMenu().querySelector('[data-dropdown] a');
}

/**
 * Gets the active menu item link.
 * @returns {HTMLElement|null} Menu item link.
 */
function getBrandLink() {
    return getMenu().querySelector('[data-brand]');
}

/**
 * Gets the active menu item link.
 * @returns {HTMLElement|null} Menu item link.
 */
function getDropdownChildLink() {
    return getMenu().querySelector('[data-child-link] a');
}

describe('luxbar', async () => {
    beforeEach(async () => {
        document.body.innerHTML = menuHtml;
        await isRendered(getMenu);

        setupLuxbarToggle();

        // intercept click on these to prevent navigation
        getActiveLink()?.addEventListener('click', (e) => {
            e.preventDefault();
        });
        getDropdownChildLink()?.addEventListener('click', (e) => {
            e.preventDefault();
        });

        await tick();
    });

    it('should uncheck checkbox when clicking menu item for new page', async () => {
        const homeLink = getActiveLink();
        const checkbox = getCheckbox();

        homeLink.click();
        await tick();

        expect(homeLink).toBeTruthy();
        expect(checkbox).toBeTruthy();
        expect(checkbox.checked).toBeFalsy();
    });

    it('should not uncheck checkbox when clicking menu item for current page', async () => {
        const brandLink = getBrandLink();
        const checkbox = getCheckbox();

        brandLink.click();
        await tick();

        expect(brandLink).toBeTruthy();
        expect(checkbox).toBeTruthy();
        expect(checkbox.checked).toBeTruthy();
    });

    it('should not uncheck checkbox when clicking dropdown menu item', async () => {
        const dropdownLink = getDropdownLink();
        const checkbox = getCheckbox();

        dropdownLink.click();
        await tick();

        expect(dropdownLink).toBeTruthy();
        expect(checkbox).toBeTruthy();
        expect(checkbox.checked).toBeTruthy();
    });

    it('should uncheck checkbox when clicking dropdown menu item for new page', async () => {
        const dropdownLink = getDropdownLink();
        const dropdownChildLink = getDropdownChildLink();
        const checkbox = getCheckbox();

        dropdownLink.click();
        dropdownChildLink.click();
        await tick();

        expect(dropdownChildLink).toBeTruthy();
        expect(checkbox).toBeTruthy();
        expect(checkbox.checked).toBeFalsy();
    });
});
