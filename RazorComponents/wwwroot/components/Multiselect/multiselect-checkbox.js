// Multiselect Checkbox JS for checkbox grid components
// Provides event-driven interaction for multiselect checkbox grids

// Event name constants
const MULTISELECT_CHANGED_EVENT = 'multiselectChanged';
const MULTISELECT_ITEM_TOGGLED_EVENT = 'multiselectItemToggled';

/**
 * Sets up a multiselect checkbox grid container.
 * @param {HTMLElement} grid - The multiselect grid element
 */
function setupMultiselectGrid(grid) {
    const checkboxes = grid.querySelectorAll('input[type="checkbox"]');
    
    checkboxes.forEach(checkbox => {
        checkbox.addEventListener('change', (event) => handleCheckboxChange(grid, event.target));
    });
}

/**
 * Handles checkbox change events, dispatching custom events for state changes.
 * @param {HTMLElement} grid - The parent grid container
 * @param {HTMLInputElement} checkbox - The changed checkbox
 */
function handleCheckboxChange(grid, checkbox) {
    const selectedValues = getSelectedValues(grid);
    
    // Dispatch item toggled event on the checkbox
    const itemEvent = new CustomEvent(MULTISELECT_ITEM_TOGGLED_EVENT, {
        bubbles: true,
        detail: {
            value: checkbox.value,
            checked: checkbox.checked,
            selectedValues: selectedValues
        }
    });
    checkbox.dispatchEvent(itemEvent);
    
    // Dispatch grid changed event on the grid container
    const gridEvent = new CustomEvent(MULTISELECT_CHANGED_EVENT, {
        bubbles: true,
        detail: {
            selectedValues: selectedValues
        }
    });
    grid.dispatchEvent(gridEvent);
}

/**
 * Gets all selected values from a grid.
 * @param {HTMLElement} grid - The multiselect grid element
 * @returns {string[]} Array of selected checkbox values
 */
function getSelectedValues(grid) {
    const checkedBoxes = grid.querySelectorAll('input[type="checkbox"]:checked');
    return Array.from(checkedBoxes).map(cb => cb.value);
}

/**
 * Programmatically selects items in a grid by value.
 * @param {HTMLElement} grid - The multiselect grid element
 * @param {string[]} values - Array of values to select
 */
function selectValues(grid, values) {
    const checkboxes = grid.querySelectorAll('input[type="checkbox"]');
    checkboxes.forEach(checkbox => {
        checkbox.checked = values.includes(checkbox.value);
    });
}

/**
 * Clears all selections in a grid.
 * @param {HTMLElement} grid - The multiselect grid element
 */
function clearSelections(grid) {
    const checkboxes = grid.querySelectorAll('input[type="checkbox"]:checked');
    checkboxes.forEach(checkbox => {
        checkbox.checked = false;
    });
}

/**
 * Selects all items in a grid.
 * @param {HTMLElement} grid - The multiselect grid element
 */
function selectAll(grid) {
    const checkboxes = grid.querySelectorAll('input[type="checkbox"]');
    checkboxes.forEach(checkbox => {
        checkbox.checked = true;
    });
}

// Initialize all grids once the DOM is fully loaded
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('[data-multiselect-grid]').forEach(setupMultiselectGrid);
});

// For testability (CommonJS environment)
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        setupMultiselectGrid,
        handleCheckboxChange,
        getSelectedValues,
        selectValues,
        clearSelections,
        selectAll,
        MULTISELECT_CHANGED_EVENT,
        MULTISELECT_ITEM_TOGGLED_EVENT
    };
}
