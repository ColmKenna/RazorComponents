/**
 * Add a new item to an edit-array container
 * @param {string} containerId - The ID of the edit-array container
 * @param {string} templateId - The ID of the template to clone
 */
function addNewItem(containerId, templateId) {
    const container = document.getElementById(containerId + '-items');
    const template = document.getElementById(templateId);
    const clone = template.content.cloneNode(true);
    
    // Get current count of items to use as new index
    const newIndex = container.children.length;
    
    // Replace '__index__' with the actual index in all input names and ids
    const allInputs = clone.querySelectorAll('input, select, textarea');
    allInputs.forEach(input => {
        if (input.name) {
            input.name = input.name.replace('__index__', newIndex);
        }
        if (input.id) {
            input.id = input.id.replace('__index__', newIndex);
        }
    });
    
    // Replace '__index__' in labels' for attribute
    const allLabels = clone.querySelectorAll('label[for]');
    allLabels.forEach(label => {
        if (label.htmlFor) {
            label.htmlFor = label.htmlFor.replace('__index__', newIndex);
        }
    });
    
    // Replace '__index__' in validation message elements
    const allValidationElements = clone.querySelectorAll('[data-valmsg-for]');
    allValidationElements.forEach(element => {
        if (element.getAttribute('data-valmsg-for')) {
            const newValue = element.getAttribute('data-valmsg-for').replace('__index__', newIndex);
            element.setAttribute('data-valmsg-for', newValue);
        }
    });
    
    // Set the ID for the new item
    const itemDiv = clone.querySelector('.edit-array-item');
    if (itemDiv) {
        const itemId = `${containerId}-item-${newIndex}`;
        itemDiv.id = itemId;
        
        // Update onclick handlers with the new ID
        const editBtn = itemDiv.querySelector('.edit-item-btn');
        if (editBtn) {
            editBtn.setAttribute('onclick', `toggleEditMode('${itemId}')`);
        }
        
        const doneBtn = itemDiv.querySelector('.done-edit-btn');
        if (doneBtn) {
            doneBtn.setAttribute('onclick', `toggleEditMode('${itemId}')`);
        }
        
        // For newly added items in display mode, show edit container first and hide display container
        const displayContainer = itemDiv.querySelector('.display-container');
        const editContainer = itemDiv.querySelector('.edit-container');
        if (displayContainer && editContainer) {
            displayContainer.style.display = 'none';
            editContainer.style.display = 'block';
        }
    }
    
    container.appendChild(clone);
    
    // Re-parse validation for the new elements if jQuery validation is available
    if (typeof $.validator !== 'undefined' && $.validator.unobtrusive) {
        $.validator.unobtrusive.parse(container);
    }
}

/**
 * Toggle between display and edit modes for an item
 * @param {string} itemId - The ID of the item to toggle
 */
function toggleEditMode(itemId) {
    const item = document.getElementById(itemId);
    if (!item) return;
    
    const displayContainer = document.getElementById(`${itemId}-display`);
    const editContainer = document.getElementById(`${itemId}-edit`);
    
    if (displayContainer && editContainer) {
        if (displayContainer.style.display === 'none') {
            // Update display with current values
            updateDisplayFromForm(itemId);
            
            // Switch from edit to display
            displayContainer.style.display = 'block';
            editContainer.style.display = 'none';
        } else {
            // Switch from display to edit
            displayContainer.style.display = 'none';
            editContainer.style.display = 'block';
        }
    }
}

/**
 * Update display values based on the current form input values
 * @param {string} itemId - The ID of the item to update
 */
function updateDisplayFromForm(itemId) {
    // This function can be expanded to update specific display elements
    // based on the form input values if needed
}
