/**
 * Add event listeners to close the menu on mobile when clicking on a luxbar menu item.
 */
function setupLuxbarToggle() {
    document.querySelectorAll('.luxbar-menu a:not([href="#"])').forEach((x) => {
        x.addEventListener('click', () => {
            const checkbox = /** @type {HTMLInputElement} */ (document.getElementById('luxbar-checkbox'));

            if (checkbox) {
                checkbox.checked = false;
            }
        });
    });
}

export default setupLuxbarToggle;
