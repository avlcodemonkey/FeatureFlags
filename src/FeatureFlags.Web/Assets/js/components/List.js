import BaseComponent from './BaseComponent.js';

/**
 * Enum for identifiers to query DOM elements.
 * @readonly
 * @enum {string}
 */
const Elements = Object.freeze({
    AddButton: 'add-button',
    RemoveButton: 'remove-button',
    Template: 'template',
    Container: 'container',
    EmptyMessage: 'empty-message',
    Item: 'item',
});

/**
 * Component to manage a list of items that can be added and deleted.
 */
class List extends BaseComponent {
    /**
     * Initialize a new instance of the List component.
     */
    constructor() {
        super();

        // set the prefix to use when querying/caching elements
        super.elementPrefix = 'list';

        // register listener for the add button
        const btn = this.getElement(Elements.AddButton);
        if (btn) {
            btn.addEventListener('click', this);
        }

        // render the empty message initially
        this.renderEmptyMessage();

        // add event listener to handle clicks on remove buttons within the container
        const container = this.getElement(Elements.Container);
        if (container) {
            container.addEventListener('click', (event) => {
                if (event.target.closest('button')?.matches(`[data-list-${Elements.RemoveButton}]`)) {
                    /** @type {HTMLInputElement} */
                    const item = event.target.closest(`[data-list-${Elements.Item}]`);
                    if (item) {
                        item.parentNode.removeChild(item);
                        this.renderEmptyMessage();
                    }
                }
            });
        }
    }

    /**
     * Clean up when removing component.
     */
    disconnectedCallback() {
        super.disconnectedCallback();
    }

    /**
     * Copy the node when the button is clicked and append it to the DOM.
     */
    async handleEvent() {
        const template = this.getElement(Elements.Template);
        const container = this.getElement(Elements.Container);

        if (template && container) {
            container.insertAdjacentHTML('beforeend', template.innerHTML);
            this.renderEmptyMessage();
        }
    }

    /**
     * Display a message if the list is empty, or hide it if there are items in the list.
     */
    renderEmptyMessage() {
        const container = this.getElement(Elements.Container);
        const emptyMessage = this.getElement(Elements.EmptyMessage);

        if (container && emptyMessage) {
            // Show the empty message if the list is empty
            if (container.children.length === 0) {
                emptyMessage.classList.remove('is-hidden');
            } else {
                emptyMessage.classList.add('is-hidden');
            }
        }
    }
}

// Define the new web component
if ('customElements' in window) {
    customElements.define('nilla-list', List);
}

export default List;
