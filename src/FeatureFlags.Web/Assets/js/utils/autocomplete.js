/**
 * Autocomplete function based on
 * autocomplete ~~ https://github.com/denis-taran/autocomplete ~~ MIT License
 */

/**
 * Generate a very complex textual ID that greatly reduces the chance of a collision with another ID or text.
 * @returns {string} A unique identifier.
 */
function uid() {
    return Date.now().toString(36) + Math.random().toString(36).substring(2); // NOSONAR
}

/**
 * Initializes autocomplete functionality on a given input element.
 * @param {object} settings - Configuration options for autocomplete.
 * @returns {{ destroy: () => void, fetch: () => void }} Object with destroy and fetch methods.
 */
const autocomplete = (settings) => {
    const doc = document;
    const container = settings.container || doc.createElement('div');
    const preventSubmit = settings.preventSubmit || 0;
    container.id = container.id || 'autocomplete-' + uid();
    const containerStyle = container.style;
    const debounceWaitMs = settings.debounceWaitMs || 0;
    const disableAutoSelect = settings.disableAutoSelect || false;
    const customContainerParent = container.parentElement;
    let items = [];
    let inputValue = '';
    let minLen = 2;
    const showOnFocus = settings.showOnFocus;
    let selected;
    let fetchCounter = 0;
    let debounceTimer;
    let destroyed = false;
    let suppressAutocomplete = false;

    if (settings.minLength !== undefined) {
        minLen = settings.minLength;
    }
    if (!settings.input) {
        throw new Error('input undefined');
    }
    const input = settings.input;
    container.className = [container.className, 'autocomplete', settings.className || ''].join(' ').trim();
    container.setAttribute('role', 'listbox');
    input.setAttribute('role', 'combobox');
    input.setAttribute('aria-expanded', 'false');
    input.setAttribute('aria-autocomplete', 'list');
    input.setAttribute('aria-controls', container.id);
    input.setAttribute('aria-owns', container.id);
    input.setAttribute('aria-activedescendant', '');
    input.setAttribute('aria-haspopup', 'listbox');
    containerStyle.position = 'absolute';

    /**
     * Detach the container from the DOM.
     */
    const detach = () => {
        container.remove();
    };

    /**
     * Clear the debounce timer if set.
     */
    const clearDebounceTimer = () => {
        if (debounceTimer) {
            window.clearTimeout(debounceTimer);
        }
    };

    /**
     * Attach the container to the DOM.
     */
    const attach = () => {
        if (!container.parentNode) {
            (customContainerParent || doc.body).appendChild(container);
        }
    };

    /**
     * Check if the autocomplete container is displayed.
     * @returns {boolean} True if container is attached to the DOM.
     */
    const containerDisplayed = () => !!container.parentNode;

    /**
     * Clear autocomplete state and hide container.
     */
    const clear = () => {
        fetchCounter++;
        items = [];
        inputValue = '';
        selected = undefined;
        input.setAttribute('aria-activedescendant', '');
        input.setAttribute('aria-expanded', 'false');
        detach();
    };

    /**
     * Update the position of the autocomplete container.
     */
    const updatePosition = () => {
        if (!containerDisplayed()) return;
        input.setAttribute('aria-expanded', 'true');
        containerStyle.height = 'auto';
        containerStyle.width = input.offsetWidth + 'px';
        let maxHeight = 0;
        let inputRect;
        const calc = () => {
            const docEl = doc.documentElement;
            const clientTop = docEl.clientTop || doc.body.clientTop || 0;
            const clientLeft = docEl.clientLeft || doc.body.clientLeft || 0;
            const scrollTop = docEl.scrollTop;
            const scrollLeft = docEl.scrollLeft;
            inputRect = input.getBoundingClientRect();
            const top = inputRect.top + input.offsetHeight + scrollTop - clientTop;
            const left = inputRect.left + scrollLeft - clientLeft;
            containerStyle.top = top + 'px';
            containerStyle.left = left + 'px';
            maxHeight = window.innerHeight - (inputRect.top + input.offsetHeight);
            if (maxHeight < 0) maxHeight = 0;
            containerStyle.top = top + 'px';
            containerStyle.bottom = '';
            containerStyle.left = left + 'px';
            containerStyle.maxHeight = maxHeight + 'px';
        };
        calc();
        calc();
        if (settings.customize && inputRect) {
            settings.customize(input, inputRect, container, maxHeight);
        }
    };

    /**
     * Redraw the autocomplete container with suggestions.
     */
    const update = () => {
        container.textContent = '';
        input.setAttribute('aria-activedescendant', '');

        // eslint-disable-next-line no-unused-vars
        let render = (item, _, __) => {
            const itemElement = doc.createElement('div');
            itemElement.textContent = item.label || '';
            return itemElement;
        };
        if (settings.render) {
            render = settings.render;
        }

        const fragment = doc.createDocumentFragment();
        for (const [index, item] of items.entries()) {
            const div = render(item, inputValue, index);
            if (div) {
                div.id = container.id + '_' + index;
                div.setAttribute('role', 'option');
                div.addEventListener('click', (ev) => {
                    suppressAutocomplete = true;
                    try {
                        settings.onSelect(item, input);
                    } finally {
                        suppressAutocomplete = false;
                    }
                    clear();
                    ev.preventDefault();
                    ev.stopPropagation();
                });
                if (item === selected) {
                    div.className += ' selected';
                    div.setAttribute('aria-selected', 'true');
                    input.setAttribute('aria-activedescendant', div.id);
                }
                fragment.appendChild(div);
            }
        }
        container.appendChild(fragment);
        if (items.length < 1) {
            if (settings.emptyMsg) {
                const empty = doc.createElement('div');
                empty.id = container.id + '_' + uid();
                empty.className = 'empty';
                empty.textContent = settings.emptyMsg;
                container.appendChild(empty);
                input.setAttribute('aria-activedescendant', empty.id);
            } else {
                clear();
                return;
            }
        }
        attach();
        updatePosition();
        updateScroll();
    };

    /**
     * Update suggestions if the container is displayed.
     */
    const updateIfDisplayed = () => {
        if (containerDisplayed()) update();
    };

    /**
     * Handle window resize event.
     */
    const resizeEventHandler = () => {
        updateIfDisplayed();
    };

    /**
     * Handle scroll event.
     * @param {Event} e - Scroll event.
     */
    const scrollEventHandler = (e) => {
        if (e.target === container) {
            e.preventDefault();
        } else {
            updateIfDisplayed();
        }
    };

    /**
     * Handle input event on the input element.
     */
    const inputEventHandler = () => {
        if (!suppressAutocomplete) {
            fetchFn(0 /* Keyboard */);
        }
    };

    /**
     * Scrolls the container to ensure the selected item is visible.
     */
    const updateScroll = () => {
        const elements = container.getElementsByClassName('selected');
        if (elements.length > 0) {
            let element = elements[0];
            if (element.offsetTop < container.scrollTop) {
                container.scrollTop = element.offsetTop;
            } else {
                const selectBottom = element.offsetTop + element.offsetHeight;
                const containerBottom = container.scrollTop + container.offsetHeight;
                if (selectBottom > containerBottom) {
                    container.scrollTop += selectBottom - containerBottom;
                }
            }
        }
    };

    /**
     * Select the previous suggestion in the list.
     */
    const selectPreviousSuggestion = () => {
        const index = items.indexOf(selected);
        selected = index === -1
            ? undefined
            : items[(index + items.length - 1) % items.length];
        updateSelectedSuggestion(index);
    };

    /**
     * Select the next suggestion in the list.
     */
    const selectNextSuggestion = () => {
        const index = items.indexOf(selected);
        if (items.length < 1) {
            selected = undefined;
        } else if (index === -1) {
            selected = items[0];
        } else {
            selected = items[(index + 1) % items.length];
        }
        updateSelectedSuggestion(index);
    };

    /**
     * Update the selected suggestion visually.
     * @param {number} index - Index of the previously selected item.
     */
    const updateSelectedSuggestion = (index) => {
        if (items.length > 0) {
            unselectSuggestion(index);
            selectSuggestion(items.indexOf(selected));
            updateScroll();
        }
    };

    /**
     * Mark a suggestion as selected.
     * @param {number} index - Index of the suggestion to select.
     */
    const selectSuggestion = (index) => {
        const element = doc.getElementById(container.id + '_' + index);
        if (element) {
            element.classList.add('selected');
            element.setAttribute('aria-selected', 'true');
            input.setAttribute('aria-activedescendant', element.id);
        }
    };

    /**
     * Unmark a suggestion as selected.
     * @param {number} index - Index of the suggestion to unselect.
     */
    const unselectSuggestion = (index) => {
        const element = doc.getElementById(container.id + '_' + index);
        if (element) {
            element.classList.remove('selected');
            element.removeAttribute('aria-selected');
            input.removeAttribute('aria-activedescendant');
        }
    };

    /**
     * Handle ArrowUp, ArrowDown, and Escape keys.
     * @param {KeyboardEvent} ev - Keyboard event.
     * @param {string} key - Key pressed.
     */
    const handleArrowAndEscapeKeys = (ev, key) => {
        const containerIsDisplayed = containerDisplayed();
        if (key === 'Escape') {
            clear();
        } else {
            if (!containerIsDisplayed || items.length < 1) return;
            key === 'ArrowUp'
                ? selectPreviousSuggestion()
                : selectNextSuggestion();
        }
        ev.preventDefault();
        if (containerIsDisplayed) {
            ev.stopPropagation();
        }
    };

    /**
     * Handle Enter key to select a suggestion.
     * @param {KeyboardEvent} ev - Keyboard event.
     */
    const handleEnterKey = (ev) => {
        if (selected) {
            if (preventSubmit === 2 /* OnSelect */) {
                ev.preventDefault();
            }
            suppressAutocomplete = true;
            try {
                settings.onSelect(selected, input);
            } finally {
                suppressAutocomplete = false;
            }
            clear();
        }
        if (preventSubmit === 1 /* Always */) {
            ev.preventDefault();
        }
    };

    /**
     * Handle keydown events on the input element.
     * @param {KeyboardEvent} ev - Keyboard event.
     */
    const keydownEventHandler = (ev) => {
        const key = ev.key;
        if (key === 'ArrowUp' || key === 'ArrowDown' || key === 'Escape') {
            handleArrowAndEscapeKeys(ev, key);
        }
        if (key === 'Enter') {
            handleEnterKey(ev);
        }
    };

    /**
     * Handle focus event on the input element.
     */
    const focusEventHandler = () => {
        if (showOnFocus) {
            fetchFn(1 /* Focus */);
        }
    };

    /**
     * Fetch suggestions using the provided fetch function.
     * @param {number} trigger - Trigger type (0: Keyboard, 1: Focus, 2: Mouse, 3: Manual).
     */
    const fetchFn = (trigger) => {
        if (input.value.length >= minLen || trigger === 1 /* Focus */) {
            clearDebounceTimer();
            debounceTimer = window.setTimeout(() => {
                return startFetch(input.value, trigger, input.selectionStart || 0);
            }, trigger === 0 /* Keyboard */ || trigger === 2 /* Mouse */ ? debounceWaitMs : 0);
        } else {
            clear();
        }
    };

    /**
     * Start fetching suggestions and update the UI.
     * @param {string} inputText - Current input value.
     * @param {number} trigger - Trigger type.
     * @param {number} cursorPos - Cursor position in the input.
     */
    const startFetch = (inputText, trigger, cursorPos) => {
        if (destroyed) return;
        const savedFetchCounter = ++fetchCounter;
        try {
            settings.fetch(inputText, (elements) => {
                if (fetchCounter === savedFetchCounter && elements) {
                    items = elements;
                    inputValue = inputText;
                    selected = (items.length < 1 || disableAutoSelect) ? undefined : items[0];
                    update();
                }
            }, trigger, cursorPos);
        } catch {
            clear();
        }
    };

    /**
     * Handle keyup events on the input element.
     * @param {KeyboardEvent} e - Keyboard event.
     */
    const keyupEventHandler = (e) => {
        if (settings.keyup) {
            settings.keyup({
                event: e,
                fetch: () => fetchFn(0 /* Keyboard */),
            });
            return;
        }
        if (!containerDisplayed() && e.key === 'ArrowDown') {
            fetchFn(0 /* Keyboard */);
        }
    };

    /**
     * Handle click events on the input element.
     * @param {MouseEvent} e - Mouse event.
     */
    const clickEventHandler = (e) => {
        if (settings.click) {
            settings.click({
                event: e,
                fetch: () => fetchFn(2 /* Mouse */),
            });
        }
    };

    /**
     * Handle blur event on the input element.
     */
    const blurEventHandler = () => {
        setTimeout(() => {
            if (doc.activeElement !== input) {
                clear();
            }
        }, 200);
    };

    /**
     * Manually trigger fetching of suggestions.
     */
    const manualFetch = () => {
        startFetch(input.value, 3 /* Manual */, input.selectionStart || 0);
    };

    // on long clicks focus will be lost and onSelect method will not be called
    container.addEventListener('mousedown', (evt) => {
        evt.stopPropagation();
        evt.preventDefault();
    });

    // If the custom autocomplete container is already appended to the DOM during widget initialization, detach it.
    detach();

    /**
     * Remove DOM elements and clear event handlers.
     */
    const destroy = () => {
        input.removeEventListener('focus', focusEventHandler);
        input.removeEventListener('keyup', keyupEventHandler);
        input.removeEventListener('click', clickEventHandler);
        input.removeEventListener('keydown', keydownEventHandler);
        input.removeEventListener('input', inputEventHandler);
        input.removeEventListener('blur', blurEventHandler);
        window.removeEventListener('resize', resizeEventHandler);
        doc.removeEventListener('scroll', scrollEventHandler, true);
        input.removeAttribute('role');
        input.removeAttribute('aria-expanded');
        input.removeAttribute('aria-autocomplete');
        input.removeAttribute('aria-controls');
        input.removeAttribute('aria-activedescendant');
        input.removeAttribute('aria-owns');
        input.removeAttribute('aria-haspopup');
        clearDebounceTimer();
        clear();
        destroyed = true;
    };

    // setup event handlers
    input.addEventListener('keyup', keyupEventHandler);
    input.addEventListener('click', clickEventHandler);
    input.addEventListener('keydown', keydownEventHandler);
    input.addEventListener('input', inputEventHandler);
    input.addEventListener('blur', blurEventHandler);
    input.addEventListener('focus', focusEventHandler);
    window.addEventListener('resize', resizeEventHandler);
    doc.addEventListener('scroll', scrollEventHandler, true);

    return {
        destroy,
        fetch: manualFetch,
    };
};

export default autocomplete;
