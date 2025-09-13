/**
 * Traverses up the DOM tree from the given node until it finds an ancestor with the specified nodeName.
 * Returns the ancestor element if found, otherwise null.
 * @param {Node} node - The starting node.
 * @param {string} nodeName - The nodeName to match (e.g., 'NILLA-DISPLAY-TOGGLE').
 * @returns {Element|null} The ancestor element matching nodeName, or null if not found.
 */
export function findClosestComponent(node, nodeName) {
    let el = node;
    while (el && el.nodeName !== nodeName) {
        el = el.parentNode;
    }
    return el && el.nodeName === nodeName ? el : null;
}
